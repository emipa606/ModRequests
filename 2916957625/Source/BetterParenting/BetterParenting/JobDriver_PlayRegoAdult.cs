using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace BetterParenting
{

    public class JobDriver_PlayRegoAdult : JobDriver
    {
        private Mote[] motesToMaintain;
        private static readonly FloatRange ToyRandomAngleOffset = new FloatRange(-5f, 5f);
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed))
            {
                return false;
            }
            if (!pawn.ReserveSittableOrSpot(job.targetB.Cell, job, errorOnFailed))
            {
                return false;
            }
            if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed && !pawn.Reserve(job.targetC, job, ((Building_Bed)base.TargetC.Thing).SleepingSlotsCount, 0, null, errorOnFailed))
            {
                return false;
            }
            return true;
        }

        public override bool CanBeginNowWhileLyingDown()
        {
            if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed)
            {
                return JobInBedUtility.InBedOrRestSpotNow(pawn, base.TargetC);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A);
            Toil watch;
            if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed)
            {
                this.KeepLyingDown(TargetIndex.C);
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C);
                yield return Toils_Bed.GotoBed(TargetIndex.C);
                watch = Toils_LayDown.LayDown(TargetIndex.C, hasBed: true, lookForOtherJobs: false);
                watch.AddFailCondition(() => !watch.actor.Awake());
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
                watch = ToilMaker.MakeToil("MakeNewToils");
            }
            watch.AddPreTickAction(delegate
            {
                WatchTickAction();
            });
            watch.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            watch.defaultCompleteMode = ToilCompleteMode.Delay;
            watch.defaultDuration = job.def.joyDuration;
            watch.handlingFacing = true;
            watch.tickAction = (Action)Delegate.Combine(watch.tickAction, (Action)delegate
            {
                if (motesToMaintain.NullOrEmpty())
                {
                    Vector3 vector = pawn.TrueCenter();
                    Vector3 v = IntVec3.North.ToVector3();
                    float num = 72f;
                    motesToMaintain = new Mote[5];
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 loc = vector + v.RotatedBy(num * (float)i + ToyRandomAngleOffset.RandomInRange) * 0.5f;
                        motesToMaintain[i] = MoteMaker.MakeStaticMote(loc, base.Map, ThingDefOf.Mote_Toy);
                    }
                }
                pawn.rotationTracker.FaceTarget(base.TargetA);
                for (int j = 0; j < motesToMaintain.Length; j++)
                {
                    motesToMaintain[j].Maintain();
                }
            });
            watch.defaultCompleteMode = ToilCompleteMode.Never;
            yield return watch;
        }

        protected virtual void WatchTickAction()
        {
            pawn.rotationTracker.FaceCell(base.TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, (Building)base.TargetThingA);
        }

        public override object[] TaleParameters()
        {
            return new object[2]
            {
            pawn,
            base.TargetA.Thing.def
            };
        }
    }
}