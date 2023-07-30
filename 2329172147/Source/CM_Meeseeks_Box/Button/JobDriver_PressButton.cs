using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class JobDriver_PressButton : JobDriver
    {
        private int useDuration = -1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useDuration, "useDuration", 0);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompHasButton>().Props.useDuration;
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            //this.FailOn(() => (base.Map.designationManager.DesignationOn(base.TargetThingA, MeeseeksDefOf.CM_Meeseeks_Box_Designation_PressButton) == null) ? true : false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            Toil toil = Toils_General.Wait(useDuration);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return toil;

            Toil finalize = new Toil();
            finalize.initAction = delegate
            {
                Pawn actor = finalize.actor;
                ThingWithComps thingWithComps = (ThingWithComps)actor.CurJob.targetA.Thing;
                for (int i = 0; i < thingWithComps.AllComps.Count; i++)
                {
                    CompHasButton compHasButton = thingWithComps.AllComps[i] as CompHasButton;
                    if (compHasButton != null)// && compHasButton.WantsPress)
                    {
                        compHasButton.DoPress(finalize.actor);
                    }
                }
                // Don't want to add a bunch of these, one day they will exist in a core mod
                //actor.records.Increment(PocketDimensionDefOf.CM_PocketDimension_Record_ButtonsPressed);
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }
    }
}
