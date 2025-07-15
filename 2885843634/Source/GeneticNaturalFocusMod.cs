using HarmonyLib;
using UnityEngine;
using Verse;

namespace GeneticNaturalFocus
{
    public class GeneticNaturalFocusMod : Mod
    {

        internal static GeneticNaturalFocusSettings Settings;


        public GeneticNaturalFocusMod(ModContentPack content) : base(content)
        {

            Harmony harmony = new("DanielWedemeyer.GeneticNaturalFocus");
            harmony.PatchAll();
            Settings = GetSettings<GeneticNaturalFocusSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {

            Listing_Standard listingStandard = new();

            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("GeneticNaturalFocus.AddOnLoad".Translate(), ref Settings.AddOnLoad);

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Genetic Natural Focus";
        }
    }

    public class GeneticNaturalFocusSettings : ModSettings
    {

        public bool AddOnLoad = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref AddOnLoad, nameof(AddOnLoad), true);
            base.ExposeData();
        }
    }
}