// ******************************************************************
//       /\ /|       @file       LordJobHostileTraderCaravanTravelAndExit.cs
//       \ V/        @brief      集群AI 敌对商队旅行和离开
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-07-11 22:42:10
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************
// Modified by llunak, l.lunak@centrum.cz .

using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;
using System.Collections.Generic;

namespace SR.ModRimWorld.RaidExtension
{
    public class LordJobHostileTraderCaravanTravelAndExit : LordJob
    {
        private IntVec3 _travelDest;
        SurpriseTimer surpriseTimer = new SurpriseTimer();

        [UsedImplicitly]
        public LordJobHostileTraderCaravanTravelAndExit()
        {
        }

        public LordJobHostileTraderCaravanTravelAndExit(IntVec3 travelDest, bool isSurprise)
        {
            _travelDest = travelDest;
            surpriseTimer.SetIsSurprise( isSurprise );
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref _travelDest, "travelDest");
            surpriseTimer.ExposeData();
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <returns></returns>
        public override StateGraph CreateGraph()
        {
            surpriseTimer.InitTimer();
            var stateGraph = new StateGraph();
            //旅行状态
            var lordToilTravel = stateGraph.AttachSubgraph(new LordJob_Travel(_travelDest).CreateGraph()).StartingToil;
            stateGraph.StartingToil = lordToilTravel;
            //离开状态
            var lordToilExitMap = new LordToil_ExitMap();
            stateGraph.AddToil(lordToilExitMap);
            //过渡 旅行到离开
            var transitionTravelToExitMap = new Transition(lordToilTravel, lordToilExitMap);
            var triggerArrived = new Trigger_Memo("TravelArrived");
            if( !surpriseTimer.IsSurprise )
                transitionTravelToExitMap.AddTrigger(triggerArrived);
            stateGraph.AddTransition(transitionTravelToExitMap);
            //过渡 任意状态到掩护逃跑
            if( !surpriseTimer.IsSurprise )
            {
                //掩护逃跑状态
                var lordToilExitMapTraderFighting = new LordToil_ExitMapTraderFighting();
                foreach (var lordToil in stateGraph.lordToils)
                {
                    var transitionAnyToExitMapAndEscortCarriers =
                        new Transition(lordToil, lordToilExitMapTraderFighting);
                    var triggerPawnLost = new Trigger_PawnLost();
                    transitionAnyToExitMapAndEscortCarriers.AddTrigger(triggerPawnLost);
                    stateGraph.AddTransition(transitionAnyToExitMapAndEscortCarriers, true);
                }
                stateGraph.AddToil(lordToilExitMapTraderFighting);
            }
            // Surprise attack.
            if( surpriseTimer.IsSurprise )
            {
                var faction = lord.faction;
                LordToil lordToilAttack = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
                Transition transitionAttack = new Transition(lordToilTravel, lordToilAttack);
                transitionAttack.AddTrigger(new Trigger_Custom( ( TriggerSignal signal ) => surpriseTimer.ActivateOn( signal )));
                transitionAttack.AddTrigger(triggerArrived);
                // LordJob_Travel adds Trigger_PawnHarmed, which takes precedence over the one here,
                // and so the caravan would prefer to just defend itself rather than start attack the moment it is attacked.
                // Remove the unsuitable trigger.
                foreach( Transition transition in stateGraph.transitions )
                    if( transition.sources.Contains( lordToilTravel ))
                        transition.triggers.RemoveAll( x => x is Trigger_PawnHarmed );
                transitionAttack.AddTrigger(new Trigger_PawnHarmed(requireInstigatorWithSpecificFaction : Faction.OfPlayer));
                var label = "SrSurpriseAttackLabel".Translate(faction.Name);
                var letter = "SrSurpriseAttackLetter".Translate(faction.def.pawnsPlural, faction.Name,
                    Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
                transitionAttack.AddPreAction(new TransitionAction_Letter(label, letter, LetterDefOf.ThreatBig));
                transitionAttack.AddPostAction(new TransitionAction_EndAllJobs());
                transitionAttack.AddPostAction(new TransitionAction_WakeAll());
                transitionAttack.AddPostAction(new TransitionAction_Custom( (Transition trans) => MakeCaravanAnimalsFlee(trans)));
                transitionAttack.AddPostAction(new TransitionAction_Custom( () => Find.TickManager.slower.SignalForceNormalSpeedShort()));
                stateGraph.AddTransition(transitionAttack);
            }
            return stateGraph;
        }

        private void MakeCaravanAnimalsFlee( Transition trans )
        {
            List<Pawn> ownedPawns = trans.target.lord.ownedPawns;
            for (int i = 0; i < ownedPawns.Count; i++)
            {
                Pawn animal = ownedPawns[i];
                if( animal.RaceProps == null || !animal.RaceProps.Animal )
                    continue;
                // Simply make all animals flee. Having them attack could look with some (sheep etc.),
                // and pack animals would be cheap loot (caravan size is smaller than normal).
                // Theoretically the faction could have attack animals, but right now those are only
                // yttakin pirates, which do not have traders. If it needs to be added, it seems
                // the best way to include those would be to check FactionDef.pawnGroupMakers
                // (it does not seem easily possible to select animals based on their race,
                // e.g. PawnKindDef.combatPower of wildboard is only 55 (less than cow's 75) and it also
                // has no trainability).
                if( animal.mindState?.mentalStateHandler == null )
                    continue;
                animal.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, forced: true, transitionSilently: true);
            }
        }
    }
}