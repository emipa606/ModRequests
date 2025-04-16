using UnityEngine;
using Verse;

namespace StyleStationMoodPatch
{
    public class StyleStationSettings : ModSettings
    {

        public bool changeHair = true;
        public bool changeBeard = true;
        public bool changeFaceTattoo = true;
        public bool changeBodyTattoo = true;
        public bool changeHairColor = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref changeHair, "changeHair");
            Scribe_Values.Look(ref changeBeard, "changeBeard");
            Scribe_Values.Look(ref changeFaceTattoo, "changeFaceTattoo");
            Scribe_Values.Look(ref changeBodyTattoo, "changeBodyTattoo");
            Scribe_Values.Look(ref changeHairColor, "changeHairColor");
            base.ExposeData();
        }
    }

    public class StyleStationPatchMod : Mod
    {
        public static StyleStationSettings settings;

        public StyleStationPatchMod(ModContentPack content) : base(content)
        {
            StyleStationPatchMod.settings = this.GetSettings<StyleStationSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap();
            listingStandard.Label("-----Base Game Settings-----");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Change Hair?", ref settings.changeHair, "Change Hair?");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Change Beard?", ref settings.changeBeard, "Change Beard?");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Change Face Tattoo?", ref settings.changeFaceTattoo, "Change Face Tattoo?");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Change Body Tattoo?", ref settings.changeBodyTattoo, "Change Body Tattoo?");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Change Hair Color?", ref settings.changeHairColor, "Change Hair Color?");
            listingStandard.Gap();
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
            StyleStationPatchMod.settings.Write();
        }

        public override string SettingsCategory()
        {
            return "Style Station Patch Settings";
        }

        public override void WriteSettings()
        {

            base.WriteSettings();
        }
    }
}
