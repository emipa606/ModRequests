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
    public static class CaravanFormingUtilityPatches
    {
        [HarmonyPatch(typeof(CaravanFormingUtility))]
        [HarmonyPatch("AllSendablePawns", MethodType.Normal)]
        public class CaravanFormingUtility_AllSendablePawns
        {
            [HarmonyPostfix]
            public static void Postfix(ref List<Pawn> __result)
            {
                __result = __result.Where(pawn => pawn.GetComp<CompMeeseeksMemory>() == null).ToList();
            }
        }
    }
}
