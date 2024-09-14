// Analyst.cs
using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace Analyst
{
    [StaticConstructorOnStartup]
    public static class Analyst
    {
        static Analyst()
        {
            Harmony val = new Harmony("Analyst");
            val.PatchAll();
        }
    }

    [DefOf]
    public static class AnalystJobDefOf
    {
        public static JobDef MechResearch;
        public static JobDef MechOperateScanner;

        static AnalystJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AnalystJobDefOf));
        }
    }

    [DefOf]
    public static class AnalystThingDefOf
    {
        public static ThingDef Mech_Analyst;

        static AnalystThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AnalystThingDefOf));
        }
    }

    public class JobDriver_MechResearch : JobDriver
    {
        private const int JobEndInterval = 4000;

        private ResearchProjectDef Project => Find.ResearchManager.currentProj;

        private Building_ResearchBench ResearchBench => (Building_ResearchBench)((JobDriver)this).TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0047: Unknown result type (might be due to invalid IL or missing references)
            if (ReservationUtility.Reserve(base.pawn, (Thing)(object)ResearchBench, base.job, 1, -1, (ReservationLayerDef)null, errorOnFailed))
            {
                if (((Thing)ResearchBench).def.hasInteractionCell)
                {
                    return ReservationUtility.ReserveSittableOrSpot(base.pawn, ((Thing)ResearchBench).InteractionCell, base.job, errorOnFailed);
                }
                return true;
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_MechResearch>(this, (TargetIndex)1);
            yield return Toils_Goto.GotoThing((TargetIndex)1, (PathEndMode)4);
            Toil research = ToilMaker.MakeToil("MakeNewToils");
            research.tickAction = delegate
            {
                Pawn actor = research.actor;
                float statValue = StatExtension.GetStatValue((Thing)(object)actor, StatDefOf.ResearchSpeed, true, -1);
                statValue *= StatExtension.GetStatValue(((JobDriver)this).TargetThingA, StatDefOf.ResearchSpeedFactor, true, -1);
                Find.ResearchManager.ResearchPerformed(statValue, actor);
            };
            ToilFailConditions.FailOn<Toil>(research, (Func<bool>)(() => Project == null));
            ToilFailConditions.FailOn<Toil>(research, (Func<bool>)(() => !Project.CanBeResearchedAt(ResearchBench, false)));
            ToilFailConditions.FailOnCannotTouch<Toil>(research, (TargetIndex)1, (PathEndMode)4);
            ToilEffects.WithEffect(research, EffecterDefOf.Research, (TargetIndex)1, (Color?)null);
            ToilEffects.WithProgressBar(research, (TargetIndex)1, (Func<float>)delegate
            {
                ResearchProjectDef project = Project;
                return (project != null) ? project.ProgressPercent : 0f;
            }, false, -0.5f, false);
            research.defaultCompleteMode = (ToilCompleteMode)3;
            research.defaultDuration = 4000;
            yield return research;
            yield return Toils_General.Wait(2, (TargetIndex)0);
        }
    }

    public class JobDriver_MechOperateScanner : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //IL_000d: Unknown result type (might be due to invalid IL or missing references)
            return ReservationUtility.Reserve(base.pawn, base.job.targetA, base.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            CompScanner scannerComp = ThingCompUtility.TryGetComp<CompScanner>(base.job.targetA.Thing);
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_MechOperateScanner>(this, (TargetIndex)1);
            ToilFailConditions.FailOnBurningImmobile<JobDriver_MechOperateScanner>(this, (TargetIndex)1);
            ToilFailConditions.FailOn<JobDriver_MechOperateScanner>(this, (Func<bool>)(() => !scannerComp.CanUseNow));
            yield return Toils_Goto.GotoThing((TargetIndex)1, (PathEndMode)4);
            Toil work = Toils_General.Wait(400); // Assuming 400 ticks for example
            work.WithEffect(EffecterDefOf.Research, TargetIndex.A);
            work.PlaySustainerOrSound(scannerComp.Props.soundWorking);
            work.tickAction = delegate
            {
                Pawn actor = work.actor;
                scannerComp.Used(actor);
            };
            yield return work;
        }
    }

    public class WorkGiver_MechOperateScanner : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ScannerDef);

        public ThingDef ScannerDef => ((WorkGiver)this).def.scannerDef;

        public override PathEndMode PathEndMode => (PathEndMode)4;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            //IL_0002: Unknown result type (might be due to invalid IL or missing references)
            //IL_0005: Unknown result type (might be due to invalid IL or missing references)
            return (Danger)3;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (!pawn.IsColonyMech)
            {
                return true;
            }
            if (((Thing)pawn).def != AnalystThingDefOf.Mech_Analyst)
            {
                return true;
            }
            List<Thing> list = ((Thing)pawn).Map.listerThings.ThingsOfDef(ScannerDef);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Faction == ((Thing)pawn).Faction)
                {
                    CompScanner val = ThingCompUtility.TryGetComp<CompScanner>(list[i]);
                    if (val != null && val.CanUseNow)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //IL_0046: Unknown result type (might be due to invalid IL or missing references)
            if (t.Faction != ((Thing)pawn).Faction)
            {
                return false;
            }
            Building val = (Building)(object)((t is Building) ? t : null);
            if (val == null)
            {
                return false;
            }
            if (ForbidUtility.IsForbidden((Thing)(object)val, pawn))
            {
                return false;
            }
            if (!ReservationUtility.CanReserve(pawn, (Thing)(object)val, 1, -1, (ReservationLayerDef)null, forced))
            {
                return false;
            }
            if (!ThingCompUtility.TryGetComp<CompScanner>((Thing)(object)val).CanUseNow)
            {
                return false;
            }
            if (FireUtility.IsBurning((Thing)(object)val))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //IL_0007: Unknown result type (might be due to invalid IL or missing references)
            return JobMaker.MakeJob(AnalystJobDefOf.MechOperateScanner, t, 1500, true);
        }
    }

    
    public class WorkGiver_MechResearcher : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                //IL_001e: Unknown result type (might be due to invalid IL or missing references)
                //IL_0023: Unknown result type (might be due to invalid IL or missing references)
                //IL_0014: Unknown result type (might be due to invalid IL or missing references)
                //IL_0019: Unknown result type (might be due to invalid IL or missing references)
                //IL_0026: Unknown result type (might be due to invalid IL or missing references)
                if (Find.ResearchManager.currentProj == null)
                {
                    return ThingRequest.ForGroup((ThingRequestGroup)1);
                }
                return ThingRequest.ForGroup((ThingRequestGroup)42);
            }
        }

        public override bool Prioritized => true;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (Find.ResearchManager.currentProj == null || !pawn.IsColonyMech)
            {
                return true;
            }
            if (((Thing)pawn).def != AnalystThingDefOf.Mech_Analyst)
            {
                return true;
            }
            return false;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //IL_0050: Unknown result type (might be due to invalid IL or missing references)
            //IL_006f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0099: Unknown result type (might be due to invalid IL or missing references)
            //IL_009e: Unknown result type (might be due to invalid IL or missing references)
            ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
            if (currentProj == null)
            {
                return false;
            }
            Building_ResearchBench val = (Building_ResearchBench)(object)((t is Building_ResearchBench) ? t : null);
            if (val == null)
            {
                return false;
            }
            if (!currentProj.CanBeResearchedAt(val, false))
            {
                return false;
            }
            if (!ReservationUtility.CanReserve(pawn, t, 1, -1, (ReservationLayerDef)null, forced) || (t.def.hasInteractionCell && !ReservationUtility.CanReserveSittableOrSpot(pawn, t.InteractionCell, forced)))
            {
                return false;
            }
            if (!IdeoUtility.Notify_PawnAboutToDo_Job(new HistoryEvent(HistoryEventDefOf.Researching, NamedArgumentUtility.Named((object)pawn, HistoryEventArgsNames.Doer))))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //IL_0007: Unknown result type (might be due to invalid IL or missing references)
            return JobMaker.MakeJob(AnalystJobDefOf.MechResearch, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return StatExtension.GetStatValue(t.Thing, StatDefOf.ResearchSpeedFactor, true);
        }
    }
}
