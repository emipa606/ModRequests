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
    public static class MentalBreakWorkerPatches
    {
        [HarmonyPatch(typeof(MentalBreakWorker))]
        [HarmonyPatch("BreakCanOccur", MethodType.Normal)]
        public static class MentalBreakWorker_BreakCanOccur
        {
            [HarmonyPostfix]
            public static void Postfix(MentalBreakWorker __instance, ref bool __result, Pawn pawn)
            {
                if (__result == true && pawn.GetComp<CompMeeseeksMemory>() != null && !__instance.def.defName.StartsWith("CM_Meeseeks_Box_MentalBreak_Meeseeks"))
                {
                    __result = false;
                }
            }
        }
    }
}
