using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Meeseeks_Box
{
    public class JobDriver_WaitNear : JobDriver
    {
        private const TargetIndex FolloweeInd = TargetIndex.A;

        private const float MaxDistance = 20.0f;

        private const float MinDistance = 10.0f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil toil = new Toil();
            toil.tickAction = delegate
            {
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                if (!base.pawn.Position.InHorDistOf(pawn.Position, MaxDistance) || !base.pawn.Position.WithinRegions(pawn.Position, base.Map, 2, TraverseParms.For(base.pawn)))
                {
                    if (!base.pawn.CanReach(pawn, PathEndMode.Touch, Danger.Deadly))
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                    else if (!base.pawn.pather.Moving || base.pawn.pather.Destination != pawn)
                    {
                        base.pawn.pather.StartPath(pawn, PathEndMode.Touch);
                    }
                }
                else if (base.pawn.Position.InHorDistOf(pawn.Position, MinDistance))
                {
                    base.pawn.pather.StopDead();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }

        public override bool IsContinuation(Job j)
        {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}
