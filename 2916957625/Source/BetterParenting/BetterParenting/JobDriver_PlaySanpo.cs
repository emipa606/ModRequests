using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BetterParenting
{
    public class JobDriver_PlaySanpo : JobDriver_BabyPlay
    {
        protected override StartingConditions StartingCondition => StartingConditions.PickupBaby;

        public override Vector3 ForcedBodyOffset
        {
            get
            {
                if (!pawn.IsCarryingPawn(base.Baby))
                {
                    return Vector3.zero;
                }
                float f = (float)Find.TickManager.TicksGame / 60f * 2f;
                if (pawn.pather.Moving)
                {
                    float f2 = Mathf.Sin(f);
                    float num = Mathf.Sign(f2);
                    return new Vector3(EasingFunctions.EaseInOutQuad(Mathf.Abs(f2) * 0.6f) * num * 0.12f, 0f, 0f);
                }
                float z = EasingFunctions.EaseInOutQuint(Mathf.Abs(Mathf.Sin(f) * 0.6f)) * 0.12f;
                return new Vector3(0f, 0f, z);
            }
        }

        public static IntVec3 TryFindWanderCell(Pawn pawn, IntVec3 root)
        {
            return RCellFinder.RandomWanderDestFor(pawn, root, 24f, null, PawnUtility.ResolveMaxDanger(pawn, Danger.None));
        }

        private void CheckEndJobSuccess()
        {
            if (roomPlayGainFactor < 0f || Find.TickManager.TicksGame % 300 == 0)
            {
                roomPlayGainFactor = BabyPlayUtility.GetRoomPlayGainFactors(base.Baby);
            }
            if (BabyPlayUtility.PlayTickCheckEnd(base.Baby, pawn, roomPlayGainFactor))
            {
                pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
            }
        }

        protected override IEnumerable<Toil> Play()
        {
            Toil playWalking = PlayWalking();
            yield return playWalking;
            yield return Toils_Jump.JumpIf(playWalking, delegate
            {
                IntVec3 intVec = TryFindWanderCell(pawn, pawn.Position);
                if (intVec.IsValid)
                {
                    job.SetTarget(TargetIndex.B, intVec);
                    return true;
                }
                return false;
            });
        }

        private Toil PlayWalking()
        {
            Toil toil = Toils_Goto.Goto(TargetIndex.B, PathEndMode.OnCell);
            toil.initAction = (Action)Delegate.Combine(toil.initAction, (Action)delegate
            {
                job.locomotionUrgency = LocomotionUrgency.Amble;
            });
            toil.tickAction = (Action)Delegate.Combine(toil.tickAction, new Action(CheckEndJobSuccess));
            ChildcareUtility.MakeBabyPlayAsLongAsToilIsActive(toil, TargetIndex.A);
            return toil;
        }
    }
}