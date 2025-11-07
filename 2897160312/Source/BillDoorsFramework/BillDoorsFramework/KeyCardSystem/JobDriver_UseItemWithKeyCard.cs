using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class JobDriver_UseItemWithKeyCard : JobDriver
    {
        private int useDuration = -1;

        private Mote warmupMote;

        private bool fromInventory;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useDuration, "useDuration", 0);
            Scribe_Values.Look(ref fromInventory, "fromInventory", false);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
            fromInventory = pawn.inventory.innerContainer.Contains(job.targetB.Thing);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) && (fromInventory || pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            CompUsable_KeyCardRequirement comp = TargetThingA.TryGetComp<CompUsable_KeyCardRequirement>();
            this.FailOn(() => !comp.CanBeUsedBy(pawn));
            this.FailOn(() => !comp.isRequiredKeyCard(TargetThingB));
            if (fromInventory)
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Thing thing2 = pawn.inventory.innerContainer.Take(TargetThingB, 1);
                    pawn.carryTracker.TryStartCarry(thing2);
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return toil;
            }
            else
            {
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            }
            yield return Toils_Goto.GotoThing(TargetIndex.A, TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
            yield return PrepareToUse();
            yield return Use();
        }

        protected Toil PrepareToUse()
        {
            Toil toil = Toils_General.Wait(useDuration, TargetIndex.A);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
            toil.handlingFacing = true;
            toil.tickAction = delegate
            {
                job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUseEffect>()?.PrepareTick();
                CompUsable_KeyCardRequirement compUsable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable_KeyCardRequirement>();
                if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
                {
                    warmupMote = MoteMaker.MakeAttachedOverlay(job.GetTarget(TargetIndex.B).Thing, compUsable.Props.warmupMote, Vector3.zero);
                }
                warmupMote?.Maintain();
                pawn.rotationTracker.FaceTarget(base.TargetA);
            };
            return toil;
        }

        protected Toil Use()
        {
            Toil use = ToilMaker.MakeToil("Use");
            use.initAction = delegate
            {
                Pawn actor = use.actor;
                CompUsable_KeyCardRequirement compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable_KeyCardRequirement>();
                compUsable.UsedBy(actor);
                CompStoredValueCard compSVC = TargetB.Thing.TryGetComp<CompStoredValueCard>();
                if (compSVC != null)
                {
                    compSVC.UsedOnce();
                    if (compSVC.RemainingCredits <= 0)
                    {
                        GenDrop.TryDropSpawn(compSVC.deactivate(), actor.Position, actor.Map, ThingPlaceMode.Near, out Thing _);
                        TargetB.Thing.Destroy();
                    }
                }
                if (compUsable.Props.finalizeMote != null)
                {
                    MoteMaker.MakeAttachedOverlay(actor.CurJob.GetTarget(TargetIndex.A).Thing, compUsable.Props.finalizeMote, Vector3.zero);
                }
                if (compUsable.Props.consumeCard)
                {
                    TargetB.Thing.Destroy();
                }
                else if (actor.carryTracker.CarriedThing != null)
                {
                    if (fromInventory)
                    {
                        actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, actor.inventory.innerContainer);
                    }
                    else
                    {
                        actor.carryTracker.TryDropCarriedThing(actor.Position, ThingPlaceMode.Near, out var _);
                    }
                }
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            return use;
        }
    }
}
