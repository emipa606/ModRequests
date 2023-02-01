using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace PawnStorages
{
    public class JobDriver_Release : JobDriver
    {
        private bool HasStation => TargetB.IsValid && TargetB.HasThing && TargetB.Thing is Building;
        private bool ReleasingSpecific => TargetC.IsValid && TargetC.HasThing && TargetC.Thing is Pawn;

        private IntVec3 ReleaseCell => HasStation ? TargetB.Thing.InteractionCell : TargetA.Cell;

        public override string GetReport()
        {
            if (HasStation && ReleasingSpecific)
            {
                return "PS_ReleaseReportA".Translate(TargetC.Pawn, TargetA.Thing, TargetB.Thing);
            }
            if (HasStation)
            {
                return "PS_ReleaseReportB".Translate(TargetA.Thing, TargetB.Thing);
            }
            if (ReleasingSpecific)
            {
                return "PS_ReleaseReportC".Translate(TargetC.Pawn, TargetA.Thing);
            }
            return "PS_ReleaseReportD".Translate(TargetA.Thing);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            if (HasStation)
            {
                this.FailOnDespawnedOrNull(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
                yield return Toils_Haul.StartCarryThing(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            }
            else
            {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            }

            Toil toil = Toils_General.Wait(60);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            yield return toil;
            Toil release = new Toil();
            release.initAction = delegate
            {
                Pawn actor = release.actor;
                var comp = TargetA.Thing.TryGetComp<CompPawnStorage>();
                if (ReleasingSpecific)
                {
                    comp.ReleasePawn(TargetC.Pawn, ReleaseCell, actor.Map);
                }
                else
                {
                    for (int num = comp.StoredPawns.Count - 1; num >= 0; num--)
                    {
                        comp.ReleasePawn(comp.StoredPawns[num], ReleaseCell, actor.Map);
                    }
                }
            };
            release.defaultCompleteMode = ToilCompleteMode.Instant;
            release.AddFinishAction(() =>
            {
                if(pawn.carryTracker.CarriedThing == TargetA.Thing) 
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
            });
            yield return release;
        }
    }
}
