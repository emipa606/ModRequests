using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace SimpleAutodoorLock
{
    //[HarmonyPatch(typeof(Building_Door), "BlocksPawn")]
    //public static class Building_Door_BlocksPawn_Patch
    //{
    //    public static void Postfix(ref Building_Door __instance, ref bool __result)
    //    {
    //        if (__instance.powerComp != null && !__instance.powerComp.PowerOn )
    //        {
    //            __result = !__instance.Open;
    //            Log.Message("BlocksPawn - instance.Open: " + __instance.Open);
    //            Log.Message("BlocksPawn - result: " + __result);
    //        }
    //    }
    //}

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

    //[HarmonyPatch(typeof(Building_Door), "CanPhysicallyPass")]
    //public static class Building_Door_CanPhysicallyPass_Patch
    //{
    //    public static void Postfix(ref Building_Door __instance, ref bool __result, Pawn p)
    //    {
    //        if (__instance.powerComp != null && !__instance.powerComp.PowerOn)
    //        {
    //            __result = __instance.FreePassage || (__instance.Open && p.HostileTo(__instance));
    //            Log.Message("CanPhysicallyPass - instance.FreePassage: " + __instance.FreePassage);
    //            Log.Message("CanPhysicallyPass - instance.Open: " + __instance.Open);
    //            Log.Message("CanPhysicallyPass - p.Hostile: " + p.HostileTo(__instance));
    //            Log.Message("CanPhysicallyPass - result: " + __result);
    //        }
    //    }
    //}
}
