// ******************************************************************
//       /\ /|       @file       LordJobPoaching.cs
//       \ V/        @brief      集群AI 偷猎
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-17 23:28:46
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Collections.Generic;
using System.Linq;

namespace SR.ModRimWorld.RaidExtension
{
    public class LordJobPoaching : LordJobSiegeBase
    {
        private static readonly IntRange WaitTime = new IntRange(500, 1000); //集合等待时间
        private Pawn _targetAnimal; //集群AI想要猎杀的动物
        SurpriseTimer surpriseTimer = new SurpriseTimer();

        public Pawn TargetAnimal => _targetAnimal;

        public LordJobPoaching()
        {
        }

        public LordJobPoaching(IntVec3 siegeSpot, Pawn targetAnimal, bool isSurprise) : base(siegeSpot)
        {
            _targetAnimal = targetAnimal;
            surpriseTimer.SetIsSurprise( isSurprise );
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _targetAnimal, "_targetAnimal");
            surpriseTimer.ExposeData();
        }

        public override StateGraph CreateGraph()
        {
            surpriseTimer.InitTimer();
            //集群AI流程状态机
            var stateGraph = new StateGraph();
            //添加流程 集结
            var lordToilStage = new LordToil_Stage(siegeSpot);
            stateGraph.AddToil(lordToilStage);
            //添加流程 偷猎
            var lordToilPoaching = new LordToilPoaching();
            stateGraph.AddToil(lordToilPoaching);
            //添加流程 带着猎物离开
            var lordToilTakePreyExit = new LordToilTakePreyExit();
            stateGraph.AddToil(lordToilTakePreyExit);
            var faction = lord.faction;
            //过渡 集合到开始偷猎
            var transition = new Transition(lordToilStage, lordToilPoaching);
            transition.AddTrigger(new Trigger_TicksPassed(WaitTime.RandomInRange));
            //唤醒成员
            transition.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(transition);
            //过渡 偷猎到带着猎物离开
            var transitionPoachingToTakePreyExit = new Transition(lordToilPoaching, lordToilTakePreyExit);
            //触发条件 目标猎物被击倒
            var triggerHuntDone = new Trigger_Custom( ( TriggerSignal signal ) => IsHuntDone( signal ));
            if( !surpriseTimer.IsSurprise )
                transitionPoachingToTakePreyExit.AddTrigger(triggerHuntDone);
            transitionPoachingToTakePreyExit.AddPreAction(new TransitionAction_Message(
                "SrTakePreyExit".Translate(faction.def.pawnsPlural.CapitalizeFirst(),
                    faction.Name), MessageTypeDefOf.ThreatSmall));
            stateGraph.AddTransition(transitionPoachingToTakePreyExit);
            // Surprise attack.
            if( surpriseTimer.IsSurprise )
            {
                LordToil lordToilAttack = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
                Transition transitionAttack = new Transition(lordToilPoaching, lordToilAttack);
                transitionAttack.AddTrigger(new Trigger_Custom( ( TriggerSignal signal ) => ShouldSurpriseAttack( signal )));
                transitionAttack.AddTrigger(triggerHuntDone);
                transitionAttack.AddTrigger(new Trigger_PawnHarmed(requireInstigatorWithSpecificFaction : Faction.OfPlayer));
                var label = "SrSurpriseAttackLabel".Translate(faction.Name);
                var letter = "SrSurpriseAttackLetter".Translate(faction.def.pawnsPlural, faction.Name,
                    Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
                transitionAttack.AddPreAction(new TransitionAction_Letter(label, letter, LetterDefOf.ThreatBig));
                transitionAttack.AddPostAction(new TransitionAction_EndAllJobs());
                transitionAttack.AddPostAction(new TransitionAction_WakeAll());
                transitionAttack.AddPostAction(new TransitionAction_Custom( () => Find.TickManager.slower.SignalForceNormalSpeedShort()));
                stateGraph.AddTransition(transitionAttack);
            }
            // Handle becoming non-hostile (from LordJob_Siege).
            LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true)
            {
                useAvoidGrid = true
            };
            stateGraph.AddToil(lordToil_ExitMap);
            Transition transitionNonHostile = new Transition(lordToilStage, lordToil_ExitMap);
            transitionNonHostile.AddSource(lordToilPoaching);
            transitionNonHostile.AddSource(lordToilTakePreyExit);
            transitionNonHostile.AddTrigger(new Trigger_BecameNonHostileToPlayer());
            transitionNonHostile.AddPreAction(new TransitionAction_Message(
                "MessageRaidersLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            stateGraph.AddTransition(transitionNonHostile);
            return stateGraph;
        }

        private bool IsHuntDone( TriggerSignal signal )
        {
            if( signal.type != TriggerSignalType.Tick )
                return false;

            const int CheckEveryTicks = 100;
            if( Find.TickManager.TicksGame % CheckEveryTicks != 0 )
                return false;

            if( TargetAnimal == null || lord.ownedPawns == null || lord.ownedPawns.Count <= 0 )
                return false;

            if( !TargetAnimal.Dead )
                return false;

            // Check for any similar animals in a radius around the original one.
            // This serves two purposes:
            // - Hunt more similar animals in the pack.
            // - Do not stop the hunt if the pack has gone manhunter.
            HashSet<ThingDef> acceptableDefs = new HashSet<ThingDef>(TargetAnimal.RaceProps.crossAggroWith
                .OrElseEmptyEnumerable().Prepend(TargetAnimal.def));
            foreach (var thing in GenRadial.RadialDistinctThingsAround(TargetAnimal.Position, lord.Map, 20f, true))
            {
                if( !(thing is Pawn animal))
                    continue;

                // Find animals that can aggro together with the original one (Pawn_MindState.GetPackMates()).
                if( animal == TargetAnimal || !acceptableDefs.Contains(animal.def) || animal.Faction != TargetAnimal.Faction )
                    continue;

                if( !lord.ownedPawns[0].IsTargetAnimalValid(animal, MiscDef.MinTargetRequireHealthScale))
                    continue;

                _targetAnimal = animal;
                return false;
            }
            return true;
        }

        private bool ShouldSurpriseAttack( TriggerSignal signal )
        {
            if( !surpriseTimer.ActivateOn( signal )) // Still ticking.
                return false;
            // Do not start the attack while the hunted animal is maddened.
            if( IsManhunter( TargetAnimal ))
                return false;
            // Or if there is another maddened animal in the vicinity.
            foreach (var thing in GenRadial.RadialDistinctThingsAround(TargetAnimal.Position, lord.Map, 30f, true))
            {
                if( !(thing is Pawn animal))
                    continue;

                if( IsManhunter( animal ))
                {
                    surpriseTimer.Delay( 2500 ); // Wait for the pawns to deal with the manhunters (performance, avoid repeated check).
                    return false;
                }
            }
            return true;
        }

        private static bool IsManhunter( Pawn animal )
        {
            // Ignore MentalStateDefOf.ManhunterPermanent, this checks if the animal got maddened by the hunt.
            return animal.InMentalState && animal.MentalStateDef == MentalStateDefOf.Manhunter;
        }
    }
}
