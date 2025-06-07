using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Message2Letter
{
    [HarmonyPatch(typeof(Messages))]
    public static class Patch_Message
    {
        [HarmonyPostfix]
        [HarmonyPatch("AcceptsMessage")]
        public static void Postfix_1(ref bool __result, string text, LookTargets lookTargets)
        {
            if (TranslationKeyObserver.currentIncident != null)
            {
                // Message is rejected, as it will be converted to a letter instead
                __result = false;

                CurrentIncident currIncident = TranslationKeyObserver.currentIncident;

                string letterLabel = currIncident.incidentDef.letterLabel;
                LetterDef letDef = currIncident.incidentDef.letterDef;

                Find.LetterStack.ReceiveLetter(letterLabel, text, letDef, lookTargets);

                // Order here matters!
                // Because we tell observer to stop observing so that
                // we are able to call .Translate (should we need it)
                // without trigger the patch while we are working.
                TranslationKeyObserver.currentIncident = null;
            }
        }
    }
}
