using HarmonyLib;
using RimWorld;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(FactionIdeosTracker), "RecalculateIdeosBasedOnPlayerPawns")]
    static class FactionIdeosTracker_RecalculateIdeosBasedOnPlayerPawns_Patch
    {
        internal static bool inRecalculate = false;

        public static void Prefix()
        {
            inRecalculate = true;
        }

        public static void Postfix()
        {
            inRecalculate = false;
        }
    }
}
