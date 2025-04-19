using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace EnchantQualityPlus.Technomancer {

    [RimWorld.DefOf]
    public static class DefOf {
        public static VFECore.Abilities.AbilityDef VPE_EnchantQuality;
        static DefOf() {
            DefOfHelper.EnsureInitializedInCtor(typeof(AbilityDefOf));
        }
    }
    public class Ability_IncreaseQuality : Ability {
        public override void Cast(params GlobalTargetInfo[] targets) {
            base.Cast(targets);

            foreach (GlobalTargetInfo target in targets) {
                CompQuality comp = target.Thing.GetInnerIfMinified().TryGetComp<CompQuality>();
                if (comp == null || comp.Quality >= MaxQuality) return;
                comp.SetQuality(comp.Quality + 1, ArtGenerationContext.Colony);
                for (int i = 0; i < 16; i++) FleckMaker.ThrowMicroSparks(target.Thing.TrueCenter(), this.pawn.Map);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true) {
            if (!base.ValidateTarget(target, showMessages)) return false;
            CompQuality comp;
            if ((comp = target.Thing.GetInnerIfMinified().TryGetComp<CompQuality>()) == null) {
                if (showMessages) Messages.Message("VPE.MustHaveQuality".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (comp.Quality >= MaxQuality) {
                if (showMessages) Messages.Message("VPE.QualityTooHigh".Translate(MaxQuality.GetLabel()), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        private QualityCategory MaxQuality {
            get {
                if (Settings.ChoiceOfMechanic == "LegacyExcellent")
                    return QualityCategory.Excellent;
                else if (Settings.ChoiceOfMechanic == "LegacyMasterwork")
                    return QualityCategory.Masterwork;
                else if (Settings.ChoiceOfMechanic == "LegacyLegendary")
                    return QualityCategory.Legendary;
                else return (QualityCategory)(int)GetPowerForPawn();
            }
        }

        public override string GetPowerForPawnDescription() => "VPE.MaxQuality".Translate(MaxQuality.GetLabel()).Colorize(UnityEngine.Color.cyan);
        public override float GetPowerForPawn() {
            if (Settings.ChoiceOfMechanic == "ScalingSquished")
                return pawn.GetStatValue(StatDefOf.PsychicSensitivity) switch {
                    <= 1.2f => (int)QualityCategory.Good,
                    <= 1.5f => (int)QualityCategory.Excellent,
                    <= 2.0f => (int)QualityCategory.Masterwork,
                    > 2.0f => (int)QualityCategory.Legendary,
                    _ => (int)QualityCategory.Normal
                };
            else
                return pawn.GetStatValue(StatDefOf.PsychicSensitivity) switch {
                    <= 1.2f => (int)QualityCategory.Good,
                    <= 1.5f => (int)QualityCategory.Excellent,
                    <= 2.5f => (int)QualityCategory.Masterwork,
                    > 2.5f => (int)QualityCategory.Legendary,
                    _ => (int)QualityCategory.Normal
                };
        }
    }
}