using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RBM_Minotaur
{
    public class JobGiver_Milking : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Jobs) protected override Job TryGiveJob(Pawn " + pawn.Name + ")"); }
            if (!pawn.Spawned)
            {
                if (MinotaurSettings.debugMessages) { Log.Message("Can't take job: pawn is not spawned"); }
                return null;
            }

            if (!pawn.IsColonist)
            {
                if (MinotaurSettings.debugMessages) { Log.Message("Can't take job: pawn is not colonist"); }
                return null;
            }

            if (pawn.abilities?.GetAbility(RBM_DefOf.RBM_Lactation)?.CanCast != true)
            {
                if (MinotaurSettings.debugMessages) { Log.Message("Can't take job: cannot cast lactate ability"); }
                return null;
            }

            if ((pawn.workSettings?.GetPriority(RBM_DefOf.BasicWorker) == 0))
            {
                if (MinotaurSettings.debugMessages) { Log.Message("Can't take job: will not do basic work"); }
                return null;
            }

            if (!RBM_Utils.TryFindMilkingSpot(pawn, out IntVec3 cellDest))
            {
                if (MinotaurSettings.debugMessages) { Log.Message("Can't take job: cannot find a milking spot"); }
                return null;
            }

            LocalTargetInfo target_location = new LocalTargetInfo(cellDest);
            LocalTargetInfo target_pawn = new LocalTargetInfo(pawn);
            Job job = JobMaker.MakeJob(RBM_DefOf.JobDriver_GotoAndUseAbility, target_pawn, target_location);
            //job.ability = pawn.abilities?.GetAbility(RBM_DefOf.RBM_Lactation);
            job.preventFriendlyFire = true;
            job.verbToUse = pawn.abilities?.GetAbility(RBM_DefOf.RBM_Lactation).verb;
            if (MinotaurSettings.debugMessages) { Log.Message("Pawn can take milking job"); }
            return job;
        }

    }
    public class JobDriver_GotoAndUseAbility : JobDriver_CastAbility
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Jobs) protected override IEnumerable<Toil> MakeNewToils()"); }
            if (ReservationUtility.Reserve(pawn, TargetB, job))
            {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);

                Toil cast = Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B);
                cast.WithProgressBar(TargetIndex.A, () => job.verbToUse.WarmupProgress);
                yield return cast;
                yield break;
            }
            Log.Warning("RBM_Minotaur: Tried to start Milking job with reserved object");
            yield break;
        }
        public override string GetReport()
        {
            return "Going to milk self.";
        }
    }
}
