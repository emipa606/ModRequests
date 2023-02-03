using Verse;
using RimWorld;

using HarmonyLib;

namespace ImprovedInsectoids
{
    [HarmonyPatch(typeof(ExecutionUtility))]
    public static class Patches_ExecutionUtility
    {
        /// <summary>
        /// Patches DoExecutionByCut to record a HistoryEvent when an insectoid is slaughtered.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ExecutionUtility.DoExecutionByCut))]
        public static bool Prefix_DoExecutionByCut(Pawn executioner, Pawn victim, int bloodPerWeight, bool spawnBlood)
        {
            if (victim.RaceProps?.Insect ?? false)
            {
                HistoryEvent historyEvent = new HistoryEvent(HistoryEventDefOf.SlaughteredInsectoid, executioner.Named(HistoryEventArgsNames.Doer));
                Find.HistoryEventsManager.RecordEvent(historyEvent, true);
            }

            return true;
        }
    }
}
