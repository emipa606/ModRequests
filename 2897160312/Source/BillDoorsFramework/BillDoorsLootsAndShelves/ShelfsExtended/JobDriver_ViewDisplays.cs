using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsLootsAndShelves
{
    internal class JobDriver_ViewDisplays : JobDriver
    {
        float variety;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Thing> potentialTargets = new List<Thing>();
            if (job.targetA.Thing.GetRoom() != null)
            {
                potentialTargets = job.targetA.Thing.GetRoom().ContainedAndAdjacentThings.Where(thing => thing is Building_Locker locker && locker.tempStorage.Any).ToList();
            }
            variety = Mathf.Clamp((float)Math.Sqrt(potentialTargets.Count), 1, 5);

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil toil = Toils_General.Wait(job.def.joyDuration, TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            toil.tickAction = delegate
            {
                WaitTickAction();
            };
            toil.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            yield return toil;

            if (job.targetB.Thing != null)
            {
                this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
                Toil toil2 = Toils_General.Wait(job.def.joyDuration, TargetIndex.B);
                toil2.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
                toil2.tickAction = delegate
                {
                    WaitTickAction();
                };

                yield return toil2;
            }

            if (job.targetC.Thing != null)
            {
                this.FailOnDestroyedNullOrForbidden(TargetIndex.C);
                yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
                Toil toil3 = Toils_General.Wait(job.def.joyDuration, TargetIndex.C);
                toil3.FailOnCannotTouch(TargetIndex.C, PathEndMode.Touch);
                toil3.tickAction = delegate
                {
                    WaitTickAction();
                };
                yield return toil3;
            }
        }

        protected void WaitTickAction()
        {
            pawn.GainComfortFromCellIfPossible();
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, variety, job.targetA.Thing as Building);
        }
    }
}
