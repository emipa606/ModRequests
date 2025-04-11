using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace LobotomyCorp.Abnos
{
    public class LCJobDriver_ApproachAbnos : JobDriver
    {
        protected static readonly TargetIndex targetId = TargetIndex.A;
        protected Thing Target => job.GetTarget(targetId).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return ReservationUtility.Reserve(this.pawn, Target, this.job);
        }

        protected virtual IEnumerable<Toil> AppendToil()
        {
            CompAbnos_Base baseComp = Target.TryGetComp<CompAbnos_Base>();
            yield return new Toil()
            {
                actor = pawn,
                initAction = () => {
                    Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
                    pawn.pather.StopDead();

                    baseComp.approached = true;
                    baseComp.actor = pawn;
                },
                tickAction = () =>
                {
                    if(baseComp.finished)
                    {
                        baseComp.finished = false;
                        pawn.jobs.curDriver.ReadyForNextToil();
                        return;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
            };
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(targetId);
            this.FailOnDestroyedOrNull(targetId);

            yield return
                Toils_Goto.GotoThing(targetId, PathEndMode.ClosestTouch)
                         .FailOnDespawnedNullOrForbidden(targetId);

            foreach(Toil t in AppendToil()) yield return t;
        }
    }
}
