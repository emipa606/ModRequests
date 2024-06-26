using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BlockUnwantedMinutiae.Patches
{
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(Quest), typeof(bool)})]
    static class GenericMessagePatch_1
    {
        static bool Prefix(string text)
        {
            return GenericMessagePatchHelper.ContainsMessage(text);
        }
    }
    
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool)})]
    static class GenericMessagePatch_2
    {
        static bool Prefix(string text)
        {
            return GenericMessagePatchHelper.ContainsMessage(text);
        }
    }
    
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(MessageTypeDef), typeof(bool)})]
    static class GenericMessagePatch_3
    {
        static bool Prefix(string text)
        {
            return GenericMessagePatchHelper.ContainsMessage(text);
        }
    }

    [HarmonyAfter(nameof(GenericMessagePatch_2))]
    [HarmonyPatch(typeof(Messages), nameof(Messages.Message))]
    [HarmonyPatch(new Type[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool)})]
    static class TaintedMessagePatch
    {
        static bool Prefix(string text)
        {
            if (LoadedModManager.GetMod<BUMMod>().GetSettings<BUMSettings>().taintedMessagePatch == false) return true;
            
            string targetMsg = "MessageDeterioratedAway".Translate(""); // blank arg so we don't have {0}
            string pattern = @".*T\)\s*" + targetMsg;
            
            Regex regex = new Regex(pattern);

            if (regex.Match(text).Length > 0) return false;
            
            return true;
        }
    }
}