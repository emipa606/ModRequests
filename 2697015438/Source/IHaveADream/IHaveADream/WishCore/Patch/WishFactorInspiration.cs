using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;

namespace HDream
{

    [HarmonyPatch(typeof(InspirationHandler), "get_StartInspirationMTBDays")]
    public static class WishFactorInspiration
    {
        public static void Postfix(ref float __result, ref InspirationHandler __instance)
        {
            int expo = __instance.pawn?.needs?.mood?.thoughts?.memories?.Memories?.FindAll(mem => mem is Thought_WishSucceed).Count ?? 0;
            __result *= Mathf.Pow(WishUtility.Def.inspirationMTBFactorPerWishSucceed, expo);
        }
    }
}
