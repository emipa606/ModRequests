using System.Collections.Generic;
using Verse.AI;
using Verse;

namespace Reload.Jobs
{
    public class JobDriver_FetchThings : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            ReservationUtility.ReserveAsManyAsPossible(pawn, job.GetTargetQueue(TargetIndex.B), job);
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);

            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch), TargetIndex.B), TargetIndex.B);
            yield return Toils_Haul.TakeToInventory(TargetIndex.B, job.count);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
        }
    }
}