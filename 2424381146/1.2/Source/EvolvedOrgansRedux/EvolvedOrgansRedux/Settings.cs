namespace EvolvedOrgansRedux {
    public class Settings : Verse.ModSettings {
        public bool CombatibilitySwitchEorVersionMidSave;
        public bool BodyPartAffinity;
        public string ChosenWorkbench;
        public System.Collections.Generic.List<string> workbenches = new System.Collections.Generic.List<string> { "Evolved Organs Redux" };
        public override void ExposeData() {
            Verse.Scribe_Values.Look(ref CombatibilitySwitchEorVersionMidSave, "CombatibilitySwitchEORVersionMidSave");
            Verse.Scribe_Values.Look(ref BodyPartAffinity, "BodyPartAffinity");
            Verse.Scribe_Values.Look(ref ChosenWorkbench, "ChosenWorkbench");
            base.ExposeData();
        }
    }

    public class EvolvedOrgansReduxSettings : Verse.Mod {
        Settings settings;
        public EvolvedOrgansReduxSettings(Verse.ModContentPack content) : base(content) {
            this.settings = GetSettings<Settings>();
        }
        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) {
            if (!settings.workbenches.Contains(settings.ChosenWorkbench))
                settings.ChosenWorkbench = settings.workbenches[0];
            Verse.Listing_Standard listingStandard = new Verse.Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Check this option if you are switching from another EOR Version to this one in the middle of a save. Please uncheck this option before starting a new save.\nThe game has to be restarted for this change to take effect.");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("CombatibilitySwitchEORVersionMidSave", ref settings.CombatibilitySwitchEorVersionMidSave);
            listingStandard.GapLine();
            listingStandard.Label("Check this option if you want to put archotech arms on lower shoulders or additional non-Evor lungs into your body.\nThe game has to be restarted for this change to take effect.");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("BodyPartAffinity", ref settings.BodyPartAffinity);
            listingStandard.GapLine();
            listingStandard.Label("Check of which mod the organ workbench should be used to create EvolvedOrgansRedux implants.\nThe game has to be restarted for this change to take effect.");
            int num = 0;
            foreach (string s in settings.workbenches) {
                if (listingStandard.RadioButton_NewTemp(settings.workbenches[num], settings.workbenches[num] == settings.ChosenWorkbench, 30f, null, null)) {
                    settings.ChosenWorkbench = settings.workbenches[num];
                }
                num++;
            }
            listingStandard.GapLine();
            listingStandard.End();
        }
        public override string SettingsCategory() {
            return "EvolvedOrgansRedux";
        }
    }
}