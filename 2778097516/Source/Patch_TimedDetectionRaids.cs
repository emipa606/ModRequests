using HarmonyLib;
using RimWorld.Planet;

namespace CaravanDetectedDontCare
{
    [HarmonyPatch(typeof(TimedDetectionRaids))]
    [HarmonyPatch("NotifyPlayer")]
    public static class Patch_TimedDetectionRaids
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
