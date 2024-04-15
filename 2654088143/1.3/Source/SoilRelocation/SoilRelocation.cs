using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace SR
{
    [StaticConstructorOnStartup]
    public static class SoilRelocation
    {
        static SoilRelocation()
        {
            Log("Initializing..");
            ToggleablePatch.ProcessPatches("UdderlyEvelyn.SoilRelocation");
            Harmony harmony = new Harmony("UdderlyEvelyn.SoilRelocation");
            harmony.PatchAll();
            if (WaterFreezes_Interop.InteropTargetIsPresent) //If WF is installed..
                harmony.Patch(AccessTools.Method(typeof(Frame), "CompleteConstruction"), new HarmonyMethod(typeof(Frame_CompleteConstruction), "Prefix")); //Clear natural water when filling water.
        }

        private static Version version = Assembly.GetAssembly(typeof(SoilRelocation)).GetName().Version;
        /// <summary>
        /// The assembly version of the mod.
        /// </summary>
        public static string Version = version.Major + "." + version.Minor + "." + version.Build;

        /// <summary>
        /// Logging function for the mod, prints the message with the appropriate method based on errorLevel, optionally ignoring the "stop logging limit".
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="errorLevel">the type of logging method to use</param>
        /// <param name="errorOnceKey">if doing ErrorOnce logging, the unique key to use (defaults to 0)</param>
        /// <param name="ignoreStopLoggingLimit">if true, resets the message count before logging the message</param>
        public static void Log(string message, ErrorLevel errorLevel = ErrorLevel.Message, int errorOnceKey = 0, bool ignoreStopLoggingLimit = false)
        {
            if (ignoreStopLoggingLimit)
                Verse.Log.ResetMessageCount();
            var text = "[Soil Relocation " + Version + "] " + message;
            switch (errorLevel)
            {
                case ErrorLevel.Message:
                    Verse.Log.Message(text);
                    break;
                case ErrorLevel.Warning:
                    Verse.Log.Warning(text);
                    break;
                case ErrorLevel.Error:
                    Verse.Log.Error(text);
                    break;
                case ErrorLevel.ErrorOnce:
                    Verse.Log.ErrorOnce(text, errorOnceKey);
                    break;
            }
        }
    }

    /// <summary>
    /// Determines which logging method to use through the SoilRelocation.Log method.
    /// </summary>
    public enum ErrorLevel
    {
        Message,
        Warning,
        Error,
        ErrorOnce,
    }

    public class SoilRelocationMod : Mod
    {
        public SoilRelocationSettings Settings;

        public SoilRelocationMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SoilRelocationSettings>();
        }

        public override string SettingsCategory()
        {
            return "Soil Relocation";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("The below settings take effect immediately, no restart required.");
            listingStandard.CheckboxLabeled("Sandbags Use Sand", ref SoilRelocationSettings.SandbagsUseSandEnabled, "Patch vanilla sandbags so they use sand in addition to cloth.");
            listingStandard.CheckboxLabeled("Fungal Gravel Uses Raw Fungus", ref SoilRelocationSettings.FungalGravelUsesRawFungusEnabled, "Patch vanilla Fungal Gravel to cost a bit of Raw Fungus to avoid it being exploitable/unbalanced.");
            listingStandard.CheckboxLabeled("Dubs Skylights Glass Uses Sand", ref SoilRelocationSettings.DubsSkylightsGlassUsesSandEnabled, "Patch Dubs Skylights glass recipes so that they use sand instead of steel.");
            listingStandard.CheckboxLabeled("Just Glass Glass Uses Sand", ref SoilRelocationSettings.JustGlassGlassUsesSandEnabled, "Patch the Just Glass glass recipe so that it uses sand instead of a stone chunk.");
            listingStandard.CheckboxLabeled("Glass+Lights Glass Uses Sand", ref SoilRelocationSettings.GlassPlusLightsGlassUsesSandEnabled, "Patch the Glass+Lights glass recipe so that it uses sand instead of a stone chunk.");
            listingStandard.CheckboxLabeled("VFE Architect Packed Dirt Costs Dirt", ref SoilRelocationSettings.VFEArchitectPackedDirtCostsDirtEnabled, "Patches VFE Architect's packed dirt recipe to cost one soil to avoid an exploit that gives you free soil.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            ToggleablePatches.Vanilla.SandbagsUseSandPatch.Enabled = SoilRelocationSettings.SandbagsUseSandEnabled;
            ToggleablePatches.DubsSkylights.GlassUsesSandPatch.Enabled = SoilRelocationSettings.DubsSkylightsGlassUsesSandEnabled;
            ToggleablePatches.JustGlass.GlassUsesSandPatch.Enabled = SoilRelocationSettings.JustGlassGlassUsesSandEnabled;
            ToggleablePatches.GlassPlusLights.GlassUsesSandPatch.Enabled = SoilRelocationSettings.GlassPlusLightsGlassUsesSandEnabled;
            ToggleablePatches.VFEArchitect.PackedDirtCostsDirt.Enabled = SoilRelocationSettings.VFEArchitectPackedDirtCostsDirtEnabled;
            ToggleablePatch.ProcessPatches("UdderlyEvelyn.SoilRelocation", "settings were updated");

            base.WriteSettings();
        }
    }
}