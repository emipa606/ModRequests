using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Price_Is_What
{
    [StaticConstructorOnStartup]
    public static class ThingPatches
    {
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("GetTooltip", MethodType.Normal)]
        public static class Thing_GetTooltip
        {
            [HarmonyPostfix]
            public static void Postfix(Thing __instance, ref TipSignal __result)
            {
                if (PlaySettingsPatches.PlaySettings_DoPlaySettingsGlobalControls.showPrice && __instance.def.hasTooltip)
                {
                    __result.text = __result.text + "\n" + "MarketValueTip".Translate() + " " + (__instance.MarketValue * __instance.stackCount);
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class PawnPatches
    {
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("GetTooltip", MethodType.Normal)]
        public static class Pawn_GetTooltip
        {
            [HarmonyPostfix]
            public static void Postfix(Thing __instance, ref TipSignal __result)
            {
                if (PlaySettingsPatches.PlaySettings_DoPlaySettingsGlobalControls.showPrice && __instance.def.hasTooltip)
                {
                    __result.text = __result.text + "\n" + "MarketValueTip".Translate() + " " + __instance.MarketValue;
                }
            }
        }
    }
}
