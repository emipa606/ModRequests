using System.Collections.Generic;
using Verse.AI;
using RimWorld;

namespace zed_0xff.CPS;

public class JobDriver_EnterMultiBuilding : JobDriver
{
    public const int EnterDelay = 60;

    private Building_MultiEnterable Building => (Building_MultiEnterable)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => !Building.CanAcceptPawn(pawn));
        yield return Toils_General.Do(delegate
                {
                Building.SelectedPawns.Add(pawn);
                });
        AddFinishAction(delegate
                {
                Building.SelectedPawns.Remove(pawn); // will be called on job cancel too
                });
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        yield return Toils_General.WaitWith(TargetIndex.A, 60, useProgressBar: true);
        yield return Toils_General.Do(delegate
                {
                Building.TryAcceptPawn(pawn);
                });
    }
}
