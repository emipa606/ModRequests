using System;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Message2Letter
{
    /// <summary>
    /// This class is used to load all incidents present in the game
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MainDelayed
    {
        static MainDelayed()
        {
            //int counter = 0;
            foreach (var incident in DefDatabase<IncidentDef>.AllDefsListForReading)
            {
                ModExtension_OnTranslationKey modExt = incident.GetModExtension<ModExtension_OnTranslationKey>();
                if (modExt != null)
                {
                    foreach (string tranKey in modExt.eventKeyList)
                    {
                        //counter++;
                        TranslationKeyObserver.translateToIncident[tranKey] = incident;
                    }
                }
            }
            //Log.Message("Added key count: " + counter.ToString());
        }
    }
}
