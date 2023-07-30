using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Meeseeks_Box
{
    public class JobDriver_ApproachTarget : JobDriver
    {
        private float maxRangeFactor = 0.95f;

        private const TargetIndex VictimIndex = TargetIndex.A;

        public Pawn Victim => (Pawn)job.GetTarget(VictimIndex).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil waitLabel = Toils_General.Label();
            Toil endLabel = Toils_General.Label();

            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);

            Toil approach = new Toil();
            approach.initAction = delegate
            {
                Job curJob = pawn.jobs.curJob;

                if (pawn == Victim || Victim == null)
                {
                    pawn.pather.StopDead();
                    ReadyForNextToil();
                }
                else
                {
                    CastPositionRequest newReq = default(CastPositionRequest);
                    newReq.caster = pawn;
                    newReq.target = Victim;
                    newReq.verb = curJob.verbToUse;
                    newReq.maxRangeFromTarget = (!Victim.Downed ? Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f) : Mathf.Min(curJob.verbToUse.verbProps.range, Victim.RaceProps.executionRange));
                    newReq.wantCoverFromTarget = false;
                    if (CastPositionFinder.TryFindCastPosition(newReq, out var dest))
                    {
                        pawn.pather.StartPath(dest, PathEndMode.OnCell);
                        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, curJob, dest);
                    }
                    //else if (pawn.PositionHeld.DistanceTo(Victim.PositionHeld) <= waitRange)
                    //{
                    //    pawn.pather.StopDead();
                    //    pawn.jobs.curDriver.JumpToToil(waitLabel);
                    //}
                    else
                    {
                        pawn.pather.StartPath(Victim, PathEndMode.Touch);
                    }
                }
            };
            approach.FailOnDespawnedOrNull(VictimIndex);
            approach.defaultCompleteMode = ToilCompleteMode.Delay;
            approach.defaultDuration = 60;
            yield return approach;

            yield return Toils_Jump.Jump(endLabel);

            yield return waitLabel;

            yield return Toils_General.Wait(60);

            yield return endLabel;
        }
    }
}
