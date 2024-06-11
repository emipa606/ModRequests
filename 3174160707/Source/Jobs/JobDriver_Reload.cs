using Reload.Components;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Reload.Jobs
{
    public class JobDriver_Reload : JobDriver
    {
        Pawn Reloader => base.TargetThingA as Pawn;

        ThingWithComps Gear => base.TargetThingB as ThingWithComps;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompReload comp = Gear?.GetComp<CompReload>();
            this.FailOn(() => comp == null);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            this.AddEndCondition(() => (this.pawn.Downed || this.pawn.Dead || this.pawn.InMentalState || FireUtility.IsBurning(this.pawn)) ? JobCondition.Incompletable : JobCondition.Ongoing);

            if (!comp.CanLoad())
                yield break;

            Toil beginToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                initAction = () =>
                {
                    comp.Props.beginSound?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{Translator.Translate("ReloadingCap")}!", 2f);
                }
            };

            Toil waitToil = new Toil
            {
                actor = pawn,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = comp.GetAdjustedReloadTime(),
            };
            waitToil = ToilEffects.WithProgressBarToilDelay(waitToil, TargetIndex.A);

            Toil reloadToil = new Toil() { defaultCompleteMode = ToilCompleteMode.Instant };
            reloadToil.AddFinishAction(delegate ()
            {
                comp.LoadAmmo(comp.Props.LoadAmount);
                if (comp.remainingAmmo < comp.MagazineCapacity)
                    comp.Props.reloadSound?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                else
                    comp.Props.endSound?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            });

            yield return beginToil;
            yield return waitToil;
            yield return reloadToil;
            yield return Toils_Jump.JumpIf(waitToil, () => comp.CanLoad());
        }
    }
}