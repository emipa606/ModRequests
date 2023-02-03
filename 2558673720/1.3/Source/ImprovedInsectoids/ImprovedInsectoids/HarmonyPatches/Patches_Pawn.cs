using Verse;
using RimWorld;

using HarmonyLib;

namespace ImprovedInsectoids
{
    [HarmonyPatch(typeof(Pawn))]
    public static class Patches_Pawn
    {
        /// <summary>
        /// Patches Kill to record a HistoryEvent when an innocent insectoid is killed.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn.Kill))]
        public static bool Prefix_Kill(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (!(dinfo is null) && dinfo.Value.Def != DamageDefOf.ExecutionCut && dinfo.Value.Instigator is Pawn killer)
            {
                if (__instance.RaceProps.Insect && HistoryEventUtility.IsKillingInnocentAnimal(killer, __instance))
                {
                    HistoryEvent historyEvent = new HistoryEvent(HistoryEventDefOf.KilledInnocentInsectoid, killer.Named(HistoryEventArgsNames.Doer));
                    Find.HistoryEventsManager.RecordEvent(historyEvent, true);
                }
            }

            return true;
        }
    }
}
