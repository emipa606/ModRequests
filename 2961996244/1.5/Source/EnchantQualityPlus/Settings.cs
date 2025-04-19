using System.Collections.Generic;
using Verse;

namespace EnchantQualityPlus {
    public class Settings : ModSettings {
        public static bool ImportantMessage20230915 = false;
        public static string ChoiceOfMechanic = "ScalingSquished";
        public List<string> ChoicesOfMechanic = new() { "ScalingSquished", "ScalingAdded", "LegacyLegendary", "LegacyMasterwork", "LegacyExcellent" };
        public override void ExposeData() {
            Scribe_Values.Look(ref ImportantMessage20230915, "ImportantMessage20230915");
            Scribe_Values.Look(ref ChoiceOfMechanic, "ChoiceOfMode");
            base.ExposeData();
        }
    }

    public class EvolvedOrgansReduxSettings : Mod {
        readonly Settings settings;
        public EvolvedOrgansReduxSettings(ModContentPack content) : base(content) {
            this.settings = GetSettings<Settings>();
        }
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) {
            Listing_Standard listingStandard = new();
            listingStandard.Begin(inRect);
            listingStandard.Label("WhichMode".Translate());
            listingStandard.Label("ScalingSquished".Translate());
            listingStandard.Label("ScalingAdded".Translate());
            listingStandard.Label("LegacyMode".Translate());
            listingStandard.Label("UntilLegendary".Translate());
            listingStandard.Label("UntilMasterwork".Translate());
            listingStandard.Label("UntilExcellent".Translate());
            listingStandard.Gap();
            for (int i = 0; i < settings.ChoicesOfMechanic.Count; i++) {
                if (listingStandard.RadioButton(settings.ChoicesOfMechanic[i], Settings.ChoiceOfMechanic == settings.ChoicesOfMechanic[i], tabIn: 30f)) {
                    Settings.ChoiceOfMechanic = settings.ChoicesOfMechanic[i];
                }
            }
            listingStandard.End();
        }
        public override string SettingsCategory() {
            return "EnchantQualityPlus";
        }
    }
}
