using HarmonyLib;
using RimWorld;
using VFED;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(CompSurveillancePillar), "Trigger")]
    public static class CompSurveillancePillar_Trigger_Patch
    {
        public static bool Prefix(CompSurveillancePillar __instance)
        {
            if (__instance.parent.Faction == Faction.OfPlayer)
            {
                return false;
            }
            return true;
        }
    }
}
