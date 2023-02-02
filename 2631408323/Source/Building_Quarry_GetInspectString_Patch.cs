using HarmonyLib;
using Quarry;
using Verse;

namespace StopQuarryingWhenFull
{
    [HarmonyPatch(typeof(Building_Quarry), nameof(Building_Quarry.GetInspectString))]
    static class Building_Quarry_GetInspectString_Patch
    {
        public static void Postfix(ref string __result, Building_Quarry __instance)
        {
            if (StopQuarryingWhenFull.QuarryIsFull(__instance))
            {
                if (__result.Length != 0)
                {
                    __result += "\n";
                }

                __result += "StopQuarryingWhenFull_FullMessage".Translate();
            }
        }
    }
}
