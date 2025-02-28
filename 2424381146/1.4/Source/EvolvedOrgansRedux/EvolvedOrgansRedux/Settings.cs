using Verse;

namespace EvolvedOrgansRedux {
    public class Settings : ModSettings {
        public static string ChosenWorkbench = "Evolved Organs Redux";
        public static bool ImportantMessage20320905 = false;
        public static bool ImportantMessage20230917 = false;
        public static bool RequireResearchProject = true;
        public static bool ChangeBestPartEfficiencySpecialWeight = false;
        public static bool ShowResearchMessageAtNewGame = false;
        public static int AmountOfArms = 4;
        public static int AmountOfLegs = 2;
        public static int AmountOfEyes = 2;
        public static int AmountOfEars = 2;
        public System.Collections.Generic.List<string> ChoicesOfWorkbenches = new() { "Evolved Organs Redux" };
        public System.Collections.Generic.List<int> ChoicesForAmountOfBodyParts = new () { 2, 4, 6, 8 };
        public override void ExposeData() {
            Scribe_Values.Look(ref ChosenWorkbench, "ChosenWorkbench", defaultValue: "Evolved Organs Redux");
            Scribe_Values.Look(ref ImportantMessage20320905, "RemovedSetting-CombatibilitySwitchEORVersionMidSave", defaultValue: false, true);
            Scribe_Values.Look(ref ImportantMessage20230917, "ImportantMessage20230917", defaultValue: false, true);
            Scribe_Values.Look(ref AmountOfArms, "AmountOfArms", defaultValue: 4);
            Scribe_Values.Look(ref AmountOfLegs, "AmountOfLegs", defaultValue: 2);
            Scribe_Values.Look(ref AmountOfEyes, "AmountOfEyes", defaultValue: 2);
            Scribe_Values.Look(ref AmountOfEars, "AmountOfEars", defaultValue: 2);
            Scribe_Values.Look(ref RequireResearchProject, "RequireResearchProject", defaultValue: true);
            Scribe_Values.Look(ref ChangeBestPartEfficiencySpecialWeight, "ChangeBestPartEfficiencySpecialWeight", defaultValue: false);
            Scribe_Values.Look(ref ShowResearchMessageAtNewGame, "ShowResearchMessageAtNewGame", defaultValue: true);
            base.ExposeData();
        }
        public Settings() {
            if (!ChoicesOfWorkbenches.Contains(ChosenWorkbench))
                ChosenWorkbench = ChoicesOfWorkbenches[0];
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
            listingStandard.GapLine(5);
            listingStandard.CheckboxLabeled("ShowResearchMessageAtNewGame".Translate(), ref Settings.ShowResearchMessageAtNewGame);
            listingStandard.GapLine(5);
            listingStandard.Label("ChosenWorkbench".Translate());
            for (int i = 0; i < settings.ChoicesOfWorkbenches.Count; i++)
                if (listingStandard.RadioButton(settings.ChoicesOfWorkbenches[i], Settings.ChosenWorkbench == settings.ChoicesOfWorkbenches[i], tabIn: 30f))
                    Settings.ChosenWorkbench = settings.ChoicesOfWorkbenches[i];
            listingStandard.GapLine(5);
            listingStandard.CheckboxLabeled("RequireResearchProject".Translate(), ref Settings.RequireResearchProject);
            listingStandard.GapLine(5);
            listingStandard.CheckboxLabeled("ChangeBestPartEfficiencySpecialWeight".Translate(), ref Settings.ChangeBestPartEfficiencySpecialWeight);
            listingStandard.GapLine(5);
            listingStandard.Label("ChoicesForAmountOfBodyParts".Translate());
            BodyPartChoiceSection(listingStandard);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
        private void BodyPartChoiceSection(Listing_Standard listingStandard) {
            Listing_Standard ls = listingStandard.BeginSection(120);
            ls.ColumnWidth = 100;
            ls.Label("Arms".Translate());
            for (int i = 0; i < settings.ChoicesForAmountOfBodyParts.Count; i++)
                if (ls.RadioButton(settings.ChoicesForAmountOfBodyParts[i].ToString(), Settings.AmountOfArms == settings.ChoicesForAmountOfBodyParts[i], tabIn: 30f))
                    Settings.AmountOfArms = settings.ChoicesForAmountOfBodyParts[i];
            ls.NewColumn();
            ls.Label("Legs".Translate());
            for (int i = 0; i < settings.ChoicesForAmountOfBodyParts.Count; i++)
                if (ls.RadioButton(settings.ChoicesForAmountOfBodyParts[i].ToString(), Settings.AmountOfLegs == settings.ChoicesForAmountOfBodyParts[i], tabIn: 30f))
                    Settings.AmountOfLegs = settings.ChoicesForAmountOfBodyParts[i];
            ls.NewColumn();
            ls.Label("Eyes".Translate());
            for (int i = 0; i < settings.ChoicesForAmountOfBodyParts.Count; i++)
                if (ls.RadioButton(settings.ChoicesForAmountOfBodyParts[i].ToString(), Settings.AmountOfEyes == settings.ChoicesForAmountOfBodyParts[i], tabIn: 30f))
                    Settings.AmountOfEyes = settings.ChoicesForAmountOfBodyParts[i];
            ls.NewColumn();
            ls.Label("Ears".Translate());
            for (int i = 0; i < settings.ChoicesForAmountOfBodyParts.Count; i++) {
                if (ls.RadioButton(settings.ChoicesForAmountOfBodyParts[i].ToString(), Settings.AmountOfEars == settings.ChoicesForAmountOfBodyParts[i], tabIn: 30f)) {
                    Settings.AmountOfEars = settings.ChoicesForAmountOfBodyParts[i];
                }
            }
            listingStandard.EndSection(ls);
        }
        public override string SettingsCategory() {
            return "EvolvedOrgansRedux";
        }
    }
}