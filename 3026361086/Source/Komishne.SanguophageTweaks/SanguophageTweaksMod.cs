using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class SanguophageTweaksSettings : ModSettings
    {
        protected static bool EnableSkipVictimBodySizeEffectOnBiterGainsDefault = true;
        public static bool EnableSkipVictimBodySizeEffectOnBiterGains =
            EnableSkipVictimBodySizeEffectOnBiterGainsDefault;

        protected static bool UseThisModsFormulaForAnimalBloodfeedingBloodLossDefault = true;
        public static bool UseThisModsFormulaForAnimalBloodfeedingBloodLoss =
            UseThisModsFormulaForAnimalBloodfeedingBloodLossDefault;

        protected static bool EnableHemogenBleedingDefault = true;
        public static bool EnableHemogenBleeding = EnableHemogenBleedingDefault;

        protected static bool EnableDebugModeDefault = false;
        public static bool EnableDebugMode = EnableDebugModeDefault;

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref EnableSkipVictimBodySizeEffectOnBiterGains,
                /*label=*/"enableSkipVictimBodySizeEffectOnBiterGains",
                /*defaultValue=*/EnableSkipVictimBodySizeEffectOnBiterGainsDefault,
                /*forceSave=*/true);

            Scribe_Values.Look(
                ref UseThisModsFormulaForAnimalBloodfeedingBloodLoss,
                /*label=*/"useThisModsFormulaForAnimalBloodfeedingBloodLoss",
                /*defaultValue=*/UseThisModsFormulaForAnimalBloodfeedingBloodLossDefault,
                /*forceSave=*/true);

            Scribe_Values.Look(
                ref EnableHemogenBleeding,
                /*label=*/"enableHemogenBleeding",
                /*defaultValue=*/EnableHemogenBleedingDefault,
                /*forceSave=*/true);

            Scribe_Values.Look(
                ref EnableDebugMode,
                /*label=*/"enableDebugMode",
                /*defaultValue=*/EnableDebugModeDefault,
                /*forceSave=*/true);
            base.ExposeData();
        }
    }

    public class SanguophageTweaksMod : Mod
    {
        public SanguophageTweaksSettings Settings;

        public SanguophageTweaksMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SanguophageTweaksSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            
            listingStandard.CheckboxLabeled(
                "KOM.SanguophageTweaks.Settings.EnableSkipVictimBodySizeEffectOnBiterGains.Label".Translate(),
                ref SanguophageTweaksSettings.EnableSkipVictimBodySizeEffectOnBiterGains,
                "KOM.SanguophageTweaks.Settings.EnableSkipVictimBodySizeEffectOnBiterGains.Tooltip".Translate());

            listingStandard.CheckboxLabeled(
                "KOM.SanguophageTweaks.Settings.UseThisModsFormulaForAnimalBloodfeedingBloodLoss.Label".Translate(),
                ref SanguophageTweaksSettings.UseThisModsFormulaForAnimalBloodfeedingBloodLoss,
                "KOM.SanguophageTweaks.Settings.UseThisModsFormulaForAnimalBloodfeedingBloodLoss.Tooltip".Translate(),
                /*height=*/40f);

            listingStandard.CheckboxLabeled(
                "KOM.SanguophageTweaks.Settings.EnableHemogenBleeding.Label".Translate(),
                ref SanguophageTweaksSettings.EnableHemogenBleeding,
                "KOM.SanguophageTweaks.Settings.EnableHemogenBleeding.Tooltip".Translate());

            listingStandard.CheckboxLabeled(
                "KOM.SanguophageTweaks.Settings.EnableDebugMode.Label".Translate(),
                ref SanguophageTweaksSettings.EnableDebugMode,
                "KOM.SanguophageTweaks.Settings.EnableDebugMode.Tooltip".Translate());

            //listingStandard.Label("exampleFloatExplanation");
            //Settings.exampleFloat = listingStandard.Slider(Settings.exampleFloat, 100f, 300f);

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "KOM.SanguophageTweaks.Settings.Category".Translate();
        }
    }
}
