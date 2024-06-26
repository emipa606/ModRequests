using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BlockUnwantedMinutiae.Patches
{
    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter))]
    [HarmonyPatch(new Type[] { typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string)})]
    static class GenericLetterPatch_1
    {
        static bool Prefix(TaggedString label)
        {
            return GenericMessagePatchHelper.ContainsLetter(label.ToString());
        }
    }
    
    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter))]
    [HarmonyPatch(new Type[] { typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(string)})]
    static class GenericLetterPatch_2
    {
        static bool Prefix(TaggedString label)
        {
            return GenericMessagePatchHelper.ContainsLetter(label.ToString());
        }
    }
    
    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter))]
    [HarmonyPatch(new Type[] { typeof(Letter), typeof(string)})]
    static class GenericLetterPatch_3
    {
        static bool Prefix(Letter let)
        {
            return GenericMessagePatchHelper.ContainsLetter(let.Label.ToString());
        }
    }
}