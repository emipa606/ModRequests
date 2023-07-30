using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class JobDriverPatches
    {
        [HarmonyPatch(typeof(JobDriver))]
        [HarmonyPatch("CheckCurrentToilEndOrFail", MethodType.Normal)]
        public class JobDriver_CheckCurrentToilEndOrFail
        {
            private static bool fudgeTime = false;

            [HarmonyPrefix]
            public static void Prefix(JobDriver __instance)
            {
                fudgeTime = (__instance.pawn.GetComp<CompMeeseeksMemory>() != null);
                DesignatorUtility.getFudgedForToilCheck = fudgeTime;
            }

            [HarmonyFinalizer]
            public static void Finalizer()
            {
                fudgeTime = false;
                DesignatorUtility.getFudgedForToilCheck = fudgeTime;
            }
        }
    }
}
