using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Verse.Sound;
using System.Linq;
using Verse.AI;
using Verse.AI.Group;

namespace LobotomyCorp.Abnos
{
    public class CompAbnos_ScorchedGirl : CompAbnos_Base
    {

        private Pawn victim;
        private static readonly JobDef approachTarget = new JobDef() {
            defName = "LCJob_ApproachPawn",
            driverClass = typeof(LCJobDriver_ApproachPawnExplosion),
            reportString = "Approach TargetA",
            allowOpportunisticPrefix = true,
        };

        public bool arrived = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            action = new Command_Target()
            {
                targetingParams = new TargetingParameters()
                {
                    validator = t =>
                    {
                        if (t == null || !t.IsValid ||
                            !t.HasThing || !(t.Thing is Pawn) ||
                            !(t.Thing as Pawn).RaceProps.Humanlike) return false;
                        return true;
                    },
                },
                //defaultLabel = parent.Label + "を使用する",
                icon = parent.def.uiIcon,
                action = t => {
                    if (actor != null) return;

                    Pawn p = t.Thing as Pawn;
                    Thing abnos = Cache.GetCoreAbnos();
                    if (abnos == null) return;

                    Verse.AI.Job tmpjob = JobMaker.MakeJob(LCDefOf.LCJob_ApproachAbnos, abnos);
                    p.jobs.TryTakeOrderedJob(tmpjob);
                },
            };

            victim = null;
        }

        public override void PostDeSpawn(Map map)
        {
            victim = null;
        }


        public override void CompTick()
        {
            base.CompTick();
            if (IsTick) return;
            if (parent.Faction != Faction.OfPlayer) parent.SetFaction(Faction.OfPlayer);

            if (IsManhunter && parentPawn.CurJobDef != approachTarget)
            {
                victim = GetRandomPawn();
                parentPawn.ClearAllReservations();
                Job tmp = JobMaker.MakeJob(approachTarget, victim);
                parentPawn.jobs.TryTakeOrderedJob(tmp);
            }
            

            Log.Message( (parentPawn.CurJob?.def.defName??"none")+ " : " + victim?.Label ?? "none");

        }

        public override IEnumerable<Gizmo> CoreGizmos()
        {
            yield return action;
            yield return new Command_Action()
            {
                defaultLabel = "Manhunter",
                action = () => {
                    parentPawn.MakeManhunter();
                },
            };
        }

        private bool IsManhunter => //true;
            parentPawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Manhunter;

        private Pawn GetRandomPawn()
        {
            //return Find.CurrentMap.PlayerPawnsForStoryteller.First();
            return Find.CurrentMap.PlayerPawnsForStoryteller.RandomElement();
        }

        private class LCJobDriver_ApproachPawnExplosion : LCJobDriver_ApproachAbnos
        {
            protected override IEnumerable<Toil> AppendToil()
            {
                yield return new Toil()
                {
                    actor = pawn,
                    initAction = () => {
                        GenExplosion.DoExplosion(
                            pawn.Position, pawn.Map, 3f, DamageDefOf.Flame,
                            pawn, 200
                            );
                        pawn.Kill(null);

                    },
                };
            }
            protected override IEnumerable<Toil> MakeNewToils()
            {
                /*
                this.FailOnDespawnedNullOrForbidden(targetId);
                this.FailOnDestroyedOrNull(targetId);
                */

                Toil go = Toils_Goto.GotoThing(targetId, PathEndMode.ClosestTouch);
                yield return go;

                
                foreach (Toil t in AppendToil()) yield return t;
            }
        }

    }


}
