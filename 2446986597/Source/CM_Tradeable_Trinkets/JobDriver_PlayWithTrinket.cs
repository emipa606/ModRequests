using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CM_Tradeable_Trinkets
{
    public class JobDriver_PlayWithTrinket : JobDriver
    {
        protected Thing Trinket => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return (pawn.Reserve(Trinket, job, 1, -1, null, errorOnFailed));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            float joyGainFactor = Trinket.GetStatValue(StatDefOf.JoyGainFactor);

            //Log.Message(string.Format("JobDriver_PlayWithTrinket.MakeNewToils: {0} playing with {1}, joyGainFactor: {2}", pawn, Trinket, joyGainFactor));

            Toil reserveTrinket = Toils_Reserve.Reserve(TargetIndex.A);
            yield return reserveTrinket;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.A);

            Toil toil = new Toil();
            toil.tickAction = delegate
            {
                if (pawn.IsHashIntervalTick(60))
                {
                    Pawn socialTarget = FindClosePawn();
                    if (socialTarget != null)
                    {
                        base.pawn.rotationTracker.FaceCell(socialTarget.Position);
                    }
                }

                base.pawn.GainComfortFromCellIfPossible();

                JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, joyGainFactor);
            };
            toil.socialMode = RandomSocialMode.SuperActive;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = job.def.joyDuration;
            toil.handlingFacing = true;
            yield return toil;
        }

        private Pawn FindClosePawn()
        {
            IntVec3 position = pawn.Position;
            for (int i = 0; i < 24; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(base.Map))
                {
                    Thing thing = intVec.GetThingList(base.Map).Find((Thing x) => x is Pawn);
                    if (thing != null && thing != pawn && GenSight.LineOfSight(position, intVec, base.Map))
                    {
                        return (Pawn)thing;
                    }
                }
            }
            return null;
        }
    }
}
