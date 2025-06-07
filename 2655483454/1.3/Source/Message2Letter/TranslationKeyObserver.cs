using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Message2Letter
{
    public static class TranslationKeyObserver
    {
        public static Dictionary<string, IncidentDef> translateToIncident = new Dictionary<string, IncidentDef>();
        public static bool ShouldBeObservingNow => currentIncident == null;

        // Medium between `Patch_TranslatorForrmattedString` to
        // `Patch_Message`
        public static CurrentIncident currentIncident = null;

    }

    public class CurrentIncident
    {
        public IncidentDef incidentDef;
        public string key;
        public NamedArgument[] namedArguments;
    }
}
