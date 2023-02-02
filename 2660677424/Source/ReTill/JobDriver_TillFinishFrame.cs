using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReTill
{
    public class JobDriver_TillFinishFrame : JobDriver
    {
        private const int JobEndInterval = 5000;

        private Frame Frame => (Frame)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return MakeTillSoilToil(this, this.Map, false);
        }

        public static Toil MakeTillSoilToil(JobDriver driver, Map map, bool removeOnBotch)
        {
            var build = new Toil();
            build.initAction = delegate
            {
                GenClamor.DoClamor(build.actor, 15f, ClamorDefOf.Construction);
            };
            build.tickAction = delegate
            {
                var actor = build.actor;
                var frame = (Frame)driver.job.targetA.Thing;
                var workAmount = actor.GetStatValue(StatDefOf.PlantWorkSpeed) * 1.7f;
                var workToBuild = frame.WorkToBuild;
                if (actor.Faction == Faction.OfPlayer)
                {
                    float statValue = actor.GetStatValue(StatDefOf.PlantHarvestYield);
                    if (!TutorSystem.TutorialMode && Rand.Value < 1f - Mathf.Pow(statValue, workAmount / workToBuild))
                    {
                        var cell = frame.Position;
                        frame.FailConstruction(actor);
                        if (removeOnBotch)
                        {
                            cell.GetFirstThing(map, ReTillDefOf.VCE_TilledSoil.blueprintDef)?.Destroy(DestroyMode.Cancel);
                        }

                        driver.ReadyForNextToil();
                        return;
                    }
                }
                map.snowGrid.SetDepth(frame.Position, 0f);
                frame.workDone += workAmount;
                if (frame.workDone >= workToBuild)
                {
                    frame.CompleteConstruction(actor);
                    driver.ReadyForNextToil();
                }
            };
            build.WithEffect(() => ((Frame)build.actor.jobs.curJob.targetA.Thing).ConstructionEffect, TargetIndex.A);
            build.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            build.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            build.FailOn(() => !GenConstruct.CanConstruct(driver.job.targetA.Thing, driver.pawn));
            build.defaultCompleteMode = ToilCompleteMode.Delay;
            build.defaultDuration = JobEndInterval;
            build.activeSkill = () => SkillDefOf.Plants;
            return build;
        }
    }
}
