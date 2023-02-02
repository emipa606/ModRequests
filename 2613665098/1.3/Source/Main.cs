using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace HediffApplier
{
    [StaticConstructorOnStartup]
    internal class Main
    {
        static Main()
        {
            try
            {
                var harmony = new Harmony("Harmony_FemaleHediff");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Log.Error($"Error during startup:\n{e}");
            }

        }

    }
    [StaticConstructorOnStartup]
    public class AP_GenderedRaidHarPatch : Mod
    {
        private static AP_GenderedRaidHarPatch _instance;
        public static AP_GenderedRaidSettings settings;
        public static AP_GenderedRaidHarPatch Instance => AP_GenderedRaidHarPatch._instance;

        public AP_GenderedRaidHarPatch(ModContentPack content)
          : base(content)
        {
            new Harmony("AP_GenderedRaid").PatchAll();
            AP_GenderedRaidHarPatch._instance = this;
            AP_GenderedRaidHarPatch.settings = base.GetSettings<AP_GenderedRaidSettings>();
            AP_GenderedRaidSettings.ApplySettings();

        }
        public override string SettingsCategory()
        {
            return "Sex Matters";
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            AP_GenderedRaidSettings.DoWindowContents(inRect);
        }
    }

    public class AP_GenderedRaidSettings : ModSettings
    {
        public static float factor=0.90f;
        public static bool allowprisonerslaves = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref factor, "factorgender", 0.90f, true);
            Scribe_Values.Look<bool>(ref allowprisonerslaves, "allowprisonerslaves", false, true);
        }
        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap();
            listing_Standard.Label($"Current male spawn ratio for raids: {factor:0.00}", -1f, "Percentage of raid pawns generated as male. Default is 90%.");
            factor = (listing_Standard.Slider(factor, 0.50f, 1.0f));
            if (ModsConfig.IdeologyActive)
            { 
                listing_Standard.CheckboxLabeled("Allow slaves to be affected by Female Presence", ref allowprisonerslaves, null); 
            }
            listing_Standard.End();
            Rect rect = inRect.BottomPart(0.1f).LeftPart(0.1f);
            Rect rect2 = inRect.BottomPart(0.1f).RightPart(0.1f);
            bool flag = Widgets.ButtonText(rect, "Apply Settings", true, true, true);
            bool flag2 = flag;
            if (flag2)
            {
                AP_GenderedRaidSettings.ApplySettings();
            }
            bool flag3 = Widgets.ButtonText(rect2, "Reset Settings", true, true, true);
            bool flag4 = flag3;
            if (flag4)
            {
                AP_GenderedRaidSettings.ResetFactor();
            }
        }
        public static void ApplySettings()
        {
            HarmonyPatches.gfactor = factor;
            ThoughtWorker_GenderRatio.allowprisonerslaves =allowprisonerslaves;
        }
        public static void ResetFactor()
        {
            factor = 0.9f;
            allowprisonerslaves = false;
        }
    }
}
