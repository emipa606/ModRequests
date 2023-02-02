using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ReTill
{
    public class JobDriver_PlaceAndBuildTilledSoil : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_General.Do(delegate
            {
                if (!GenConstruct.CanPlaceBlueprintAt(ReTillDefOf.VCE_TilledSoil, job.targetA.Cell, Rot4.North, Map))
                {
                    EndJobWith(JobCondition.Incompletable);
                }
                else
                {
                    TargetThingA = GenConstruct.PlaceBlueprintForBuild_NewTemp(ReTillDefOf.VCE_TilledSoil, job.targetA.Cell, Map, Rot4.North, pawn.Faction, null);
                }
            });

            yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.A, TargetIndex.A);

            yield return JobDriver_TillFinishFrame.MakeTillSoilToil(this, this.Map, true);
        }
    }
}
