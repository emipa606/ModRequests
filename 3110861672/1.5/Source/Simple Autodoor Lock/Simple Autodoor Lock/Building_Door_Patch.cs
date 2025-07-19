using HarmonyLib;
using RimWorld;

namespace SimpleAutodoorLock
{
    [HarmonyPatch(typeof(Building_Door), "PawnCanOpen")]
    public static class Building_Door_PawnCanOpen_Patch
    {
        public static void Postfix(ref Building_Door __instance, ref bool __result)
        {
            if (__result == true && __instance.powerComp != null && !__instance.powerComp.PowerOn)
            {
                __result = false;
            }
        }
    }
}
