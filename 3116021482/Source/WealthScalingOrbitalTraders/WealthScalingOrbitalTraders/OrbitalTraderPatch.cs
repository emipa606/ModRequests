using System;
using HarmonyLib;
using RimWorld;
using Verse;
using Unity;
using XmlExtensions.Setting;
using XmlExtensions;

namespace WealthScalingOrbitalTraders
{
    [StaticConstructorOnStartup]
    public static class WealthScalingOrbitalTraders
    {
        static WealthScalingOrbitalTraders() //our constructor
        {
            Log.Message("Wealth-Scaling Orbital Traders has loaded successfully!");

            WSOTPatchApplier.DoPatching();

        }
    }
    public static class WSOTPatchApplier
    {
        // make sure DoPatching() is called at start either by
        // the mod loader or by your injector

        public static void DoPatching()
        {
            var harmony = new Harmony("daxturus.scalingorbitaltraders");
            harmony.PatchAll();
        }
    }
    public class WealthText : SettingContainer
    {

        protected override void DrawSettingContents(UnityEngine.Rect inRect, string selectedMod)
        {
            GameFont currFont = Verse.Text.Font;
            Verse.Text.Font = GameFont.Small;
            float stockCalc = Trader_GenerateThings_Patch.CalculateAddedItemsDefault(float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthTest")));
            float wealthPerStock = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthPerStock"));
            float wealthMinimum = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMinWealth"));
            float stockCalc2 = (float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthTest")) - wealthMinimum) / wealthPerStock;
            Widgets.Label(inRect, "Extra Stock: "+stockCalc2.ToString()+", Adjusted: "+stockCalc.ToString());
            Verse.Text.Font = currFont;
        }

        protected override float CalculateHeight(float width, string selectedMod)
        {
            return 40 + GetDefaultSpacing();
        }
    }

    /*
    public class StockGeneratorProperty : Def
    {
        string generatorClass = "StockGenerator";
        string generatorInput = "";
    }*/
}
