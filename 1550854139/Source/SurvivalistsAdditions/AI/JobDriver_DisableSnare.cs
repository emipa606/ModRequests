using System.Collections.Generic;

using Verse.AI;

namespace SurvivalistsAdditions
{

    public class JobDriver_DisableSnare : JobDriver
    {


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(100)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);

            yield return new Toil
            {
                initAction = delegate
                {
                    Building_Snare snare = (Building_Snare)GetActor().CurJob.GetTarget(TargetIndex.A).Thing;
                    //snare.Disable();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
