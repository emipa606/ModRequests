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
    public static class WorldPawnGCPatches
    {
        [HarmonyPatch(typeof(WorldPawnGC))]
        [HarmonyPatch("GetCriticalPawnReason", MethodType.Normal)]
        public class WorldPawnGC_GetCriticalPawnReason
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, ref string __result)
            {
                if (pawn != null && pawn.GetComp<CompMeeseeksMemory>() != null)
                {
                    Logger.MessageFormat(pawn, "Meeseeks {0} marked as non-essential to world during GC pass.", pawn.GetUniqueLoadID());
                    __result = null;
                }
            }
        }
    }
}
