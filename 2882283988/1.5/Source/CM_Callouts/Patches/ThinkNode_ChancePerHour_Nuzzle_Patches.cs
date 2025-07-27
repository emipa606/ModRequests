using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts;

namespace CM_Callouts.Patches
{
    [StaticConstructorOnStartup]
    public static class ThinkNode_ChancePerHour_Nuzzle_Patches
    {
        [HarmonyPatch(typeof(ThinkNode_ChancePerHour_Nuzzle))]
        [HarmonyPatch("MtbHours", MethodType.Normal)]
        public static class ThinkNode_ChancePerHour_Nuzzle_MtbHours
        {
            [HarmonyPostfix]
            public static void Postfix(ref float __result)
            {
                if (Prefs.DevMode && CalloutMod.settings.hyperNuzzling)
                    __result = 0.01f;
            }
        }
    }
}
