using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BlockUnwantedMinutiae.Patches
{
    internal static class GenericMessagePatchHelper
    {
        internal static bool ContainsMessage(string text)
        {
            List<string> labels = LoadedModManager.GetMod<BUMMod>().GetSettings<BUMSettings>().ActiveGenericMessagePatches();

            foreach (string l in labels)
            {
                string targetMsg = ReplaceTags(l.Translate());
                Regex regex = new Regex(@".*" + targetMsg);
                
                if (regex.Match(text).Length > 0) return false;
            }
            
            return true;
        }
        
        internal static bool ContainsLetter(string text)
        {
            List<string> labels = LoadedModManager.GetMod<BUMMod>().GetSettings<BUMSettings>().ActiveGenericLetterPatches();

            foreach (string l in labels)
            {
                string targetMsg = ReplaceTags(l.Translate());
                Regex regex = new Regex(@"" + targetMsg);
                
                if (regex.Match(text).Length > 0) return false;
            }
            
            return true;
        }

        private static string ReplaceTags(string text)
        {
            Regex regex = new Regex(@"{\S*}");
            return regex.Replace(text, ".*");
        }
    }
    
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            new Harmony("BlockUnwantedMinutiae").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), nameof(Dialog_FormCaravan.DrawAutoSelectCheckbox))]
    static class DrawAutoSelectCheckboxPatch
    {
        static bool Prefix(Dialog_FormCaravan __instance)
        {
            if (LoadedModManager.GetMod<BUMMod>().GetSettings<BUMSettings>().drawAutoSelectCheckboxPatch == false) return true;
            
            Traverse.Create(__instance).Field("autoSelectTravelSupplies").SetValue(false);
            return false;
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), nameof(Dialog_FormCaravan.PostOpen))]
    static class PostOpenPatch
    {
        static bool Prefix(Dialog_FormCaravan __instance)
        {
            if (LoadedModManager.GetMod<BUMMod>().GetSettings<BUMSettings>().drawAutoSelectCheckboxPatch == true)
                Traverse.Create(__instance).Field("autoSelectTravelSupplies").SetValue(false);
            return true;
        }
    }
}