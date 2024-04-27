using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MoreArchotechGarbage
{
    public class JobDriver_CarryCorpseToBuilding : JobDriver
    {
        private const TargetIndex BuildingInd = TargetIndex.A;

        private const TargetIndex TakeeInd = TargetIndex.B;

        private Corpse Takee => (Corpse)job.GetTarget(TargetIndex.B).Thing;

        private Building_Enterable Building => (Building_Enterable)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !Building.CanAcceptPawn(Takee.InnerPawn));
            yield return Toils_General.Do(delegate
            {
                Building.SelectedPawn = Takee.InnerPawn;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(TargetIndex.A, 60, useProgressBar: true);
            yield return Toils_General.Do(delegate
            {
                Building.TryAcceptPawn(Takee.InnerPawn);
            });
        }
    }
}
