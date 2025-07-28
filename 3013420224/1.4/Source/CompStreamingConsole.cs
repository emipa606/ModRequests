using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace NoobertNebulous
{
    public class CompProperties_StreamingConsole : CompProperties
    {
        public CompProperties_StreamingConsole()
        {
            this.compClass = typeof(CompStreamingConsole);
        }
    }

    public class CompStreamingConsole : ThingComp
    {
        public Video videoToRecord;
        private CompPowerTrader powerComp;
        public bool CanUseConsoleNow
        {
            get
            {
                if (parent.Spawned && parent.Map.gameConditionManager.ElectricityDisabled)
                {
                    return false;
                }
                if (powerComp != null)
                {
                    return powerComp.PowerOn;
                }
                return true;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (myPawn.skills.GetSkill(SkillDefOf.Social)?.TotallyDisabled ?? false)
            {
                return new FloatMenuOption("NN.CannotUseIncapableOfSocial".Translate(myPawn.Named("PAWN")), null);
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return new FloatMenuOption("CannotUseReason".Translate("IncapableOfCapacity".Translate(PawnCapacityDefOf.Talking.label, myPawn.Named("PAWN"))), null);
            }
            if (!myPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Some))
            {
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            }
            if (parent.Spawned && parent.Map.gameConditionManager.ElectricityDisabled)
            {
                return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            }
            if (powerComp != null && !powerComp.PowerOn)
            {
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            }
            if (!CanUseConsoleNow)
            {
                Log.Error(string.Concat(myPawn, " could not use comm console for unknown reason."));
                return new FloatMenuOption("Cannot use now", null);
            }
            return null;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            FloatMenuOption failureReason = GetFailureReason(selPawn);
            if (failureReason != null)
            {
                yield return failureReason;
            }
            else
            {
                if (selPawn.IsVidtuber())
                {
                    if (this.videoToRecord != null)
                    {
                        yield return StartRecordingOption(selPawn);
                    }
                    yield return GetOpenVidtubeOption(selPawn);
                    yield return GetDeleteChannelOption(selPawn);
                }
                else
                {
                    yield return GetSetupChannelOption(selPawn);
                }
            }
        }

        private FloatMenuOption StartRecordingOption(Pawn negotiator)
        {
            string text = "NN.StartRecording".Translate(negotiator.Named("PAWN"));
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
            {
                Job job = JobMaker.MakeJob(NN_DefOf.NN_RecordVideo, parent);
                negotiator.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Default), negotiator, parent);
        }
        private FloatMenuOption GetSetupChannelOption(Pawn negotiator)
        {
            string text = "NN.SetupChannel".Translate(negotiator.Named("PAWN"));
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
            {
                Job job = JobMaker.MakeJob(NN_DefOf.NN_SetupChannel, parent);
                negotiator.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Default), negotiator, parent);
        }

        private FloatMenuOption GetOpenVidtubeOption(Pawn negotiator)
        {
            string text = "NN.OpenVidtube".Translate();
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
            {
                Job job = JobMaker.MakeJob(NN_DefOf.NN_OpenVidtube, parent);
                negotiator.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Default), negotiator, parent);
        }

        private FloatMenuOption GetDeleteChannelOption(Pawn negotiator)
        {
            string text = "NN.DeleteChannel".Translate(negotiator.Named("PAWN"));
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
            {
                Job job = JobMaker.MakeJob(NN_DefOf.NN_DeleteChannel, parent);
                negotiator.jobs.TryTakeOrderedJob(job);
            }, MenuOptionPriority.Default), negotiator, parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref videoToRecord, "videoToRecord");
        }
    }
}
