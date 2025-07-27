using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class RealTime_Patches
    {
        [HarmonyPatch(typeof(RealTime))]
        [HarmonyPatch("Update", MethodType.Normal)]
        public static class RealTime_Update
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                CalloutTracker.UpdateTextMoteQueuesRealTime();
            }
        }
    }
}