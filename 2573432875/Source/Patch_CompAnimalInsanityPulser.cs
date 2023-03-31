using HarmonyLib;
using RimWorld;

namespace AnimalFilthDontCare
{
    [HarmonyPatch(typeof(Alert_AnimalFilth))]
    [HarmonyPatch("GetReport")]
    public static class Patch_Alert_AnimalFilth_GetReport
    {
        public static void Postfix(ref AlertReport __result)
        {
            __result = false;
        }
    }
}
