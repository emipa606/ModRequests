using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace BetterParenting
{
    public class JobDriver_PlayRego : JobDriver
    {
        public Building_Toybox Toybox => (Building_Toybox)base.TargetThingA;
        private Mote[] motesToMaintain;
        private static readonly FloatRange ToyRandomAngleOffset = new FloatRange(-5f, 5f);
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            this.FailOnChildLearningConditions();
            this.FailOn(() => !Toybox.CanUseboxNow);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(() => !Toybox.CanUseboxNow);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.tickAction = delegate
            {
                pawn.rotationTracker.FaceTarget(base.TargetA);
                LearningUtility.LearningTickCheckEnd(pawn);
            };
            toil.handlingFacing = true;
            toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
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
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}