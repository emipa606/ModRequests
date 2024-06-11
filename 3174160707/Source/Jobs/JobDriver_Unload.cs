using Reload.Components;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Reload.Jobs
{
    public class JobDriver_Unload : JobDriver
    {
        ThingWithComps Weapon => base.TargetThingA as ThingWithComps;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompReload comp = Weapon?.TryGetComp<CompReload>();

            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => comp == null);
            this.FailOn(() => !comp.CanBeUsed);

            Toil endToil = new Toil
            {
                initAction = delegate ()
                {
                    if(Setting.EnableAmmo)
                        Utils.GenerateThingsToGround(pawn.Position, pawn.Map, comp.Props.ammoDef, comp.remainingAmmo);
                    comp.remainingAmmo = 0;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return ToilFailConditions.FailOnSomeonePhysicallyInteracting<Toil>(ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch), TargetIndex.A), TargetIndex.A);
            yield return ToilEffects.WithProgressBarToilDelay(Toils_General.Wait(comp.GetAdjustedReloadTime()), TargetIndex.A);
            yield return endToil;
        }
    }
}
