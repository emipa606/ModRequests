using Verse;
using RimWorld;
using Verse.AI;
using System.Collections.Generic;

namespace ResistanceRestraintsMod
{
    public class JobDriver_RemoveImmobility : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn targetPawn = TargetA.Thing as Pawn;

            // Ensure the job has a valid prisoner
            if (targetPawn == null)
            {
                yield break;
            }

            // Move to the prisoner
            Toil goToPrisoner = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return goToPrisoner;

            // Perform the removal
            Toil removeHediff = new Toil();
            removeHediff.initAction = () =>
            {
                Hediff immobilityHediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);
                if (immobilityHediff != null)
                {
                    targetPawn.health.RemoveHediff(immobilityHediff);
                }
            };
            removeHediff.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return removeHediff;
        }
    }
}
