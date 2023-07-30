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
    public static class ForbidUtilityPatches
    {
        [HarmonyPatch(typeof(ForbidUtility))]
        [HarmonyPatch("CaresAboutForbidden", MethodType.Normal)]
        public static class ForbidUtility_CaresAboutForbidden
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, ref bool __result)
            {
                if (__result == true && pawn.GetComp<CompMeeseeksMemory>() != null)
                {
                    __result = false;
                }
            }
        }
    }
}
