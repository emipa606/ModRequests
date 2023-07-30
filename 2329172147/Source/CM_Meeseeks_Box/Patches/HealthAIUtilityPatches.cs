using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class HealthAIUtilityPatches
    {
        [HarmonyPatch(typeof(HealthAIUtility))]
        [HarmonyPatch("ShouldEverReceiveMedicalCareFromPlayer", MethodType.Normal)]
        public static class HealthAIUtility_ShouldEverReceiveMedicalCareFromPlayer
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn)
            {
                if (!__result)
                {
                    if (DesignatorUtility.getFudgedForToilCheck || DesignatorUtility.getFudgedForWorkgiverCheck)
                        __result = true;
                }
            }
        }
    }
}
