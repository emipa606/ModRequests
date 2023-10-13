using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace BuriedAlive
{
    public class JobDriver_BuryAlive : JobDriver
    {
        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_Grave Grave
        {
            get
            {
                return (Building_Grave)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Takee, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.Grave, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOn(() => !this.Grave.Accepts(this.Takee));
            Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.Grave.GetDirectlyHeldThings().Count > 0).FailOn(() => !this.Takee.Downed).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil startCarryingTakee = Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            Toil goToThing = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Jump.JumpIf(goToThing, () => this.pawn.IsCarryingPawn(this.Takee));
            yield return goToTakee;
            yield return startCarryingTakee;
            yield return goToThing;
            Toil toil = Toils_General.Wait(500, TargetIndex.B);
            toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.ClosestTouch);
            toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return toil;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.Grave.TryAcceptThing(this.Takee, true);
                    IEnumerable<BodyPartRecord> parts = this.Takee.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).InRandomOrder();
                    foreach (BodyPartRecord part in parts)
                    {
                        this.Takee.TakeDamage(new DamageInfo(DamageDefOf.Crush, 15f, 99999f, -1f, this.Grave, part, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, false));
                    }
                    if (!this.Takee.Dead)
                    {
                        this.Takee.Kill(new DamageInfo(DamageDefOf.Crush, 99999f));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
