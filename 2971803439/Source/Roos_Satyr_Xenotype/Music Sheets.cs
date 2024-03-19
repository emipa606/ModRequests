using System.Collections.Generic;
using Verse;
using Verse.Sound;
using Verse.AI;
using RimWorld;

namespace Roos_Satyr_Xenotype
{

    public class JobDriver_UseMusicSheet : JobDriver_UseItem
    {

        protected Thing MusicSheet
        {
            get
            {
                return this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected CompMusicSheet MusicSheetComp
        {
            get
            {
                return this.MusicSheet.TryGetComp<CompMusicSheet>();
            }

        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            this.FailOn(() => !base.TargetThingA.TryGetComp<CompUsable>().CanBeUsedBy(pawn, out var _));
            yield return Toils_Goto.GotoThing(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
            yield return PrepareToUse();
            yield return Use();
            yield return Toils_General.Do(delegate
            {
                AbilityDef abilityDef = MusicSheetComp.Props.ability;
                Log.Message("Pawn: " + pawn.Name + "Used the music sheet, and gained " + abilityDef);
                Toil use = base.Use();
                this.pawn.abilities.GainAbility(abilityDef);
            });
        }
    }

    public class CompMusicSheet : CompUsable
    {
        public new CompProperties_MusicSheet Props
        {
            get
            {
                return (CompProperties_MusicSheet)this.props;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {

            JobFailReason.Clear();
            if (!selPawn.genes.HasGene(RBSF_DefOf.RBSF_Virtuoso))
            {
                JobFailReason.Is(selPawn.Name + "is not a Virtuoso", null);
                yield return new FloatMenuOption("Cannot use Music Sheet: " + JobFailReason.Reason.CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            var baseReturns = base.CompFloatMenuOptions(selPawn);
            foreach (FloatMenuOption floatMenuOption in baseReturns)
            {
                yield return floatMenuOption;
            }
            yield break;
        }
    }

    public class CompProperties_MusicSheet : CompProperties_Usable
    {

        public CompProperties_MusicSheet()
        {
            this.compClass = typeof(CompMusicSheet);
        }

        public AbilityDef ability;
    }
}
