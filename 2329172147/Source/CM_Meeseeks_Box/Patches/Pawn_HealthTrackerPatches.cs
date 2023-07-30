using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class Pawn_HealthTrackerPatches
    {
        //[HarmonyPatch(typeof(Pawn_HealthTracker))]
        //[HarmonyPatch("MakeDowned", MethodType.Normal)]
        //public class Pawn_HealthTracker_MakeDowned
        //{
        //    [HarmonyPrefix]
        //    public static void Prefix(Hediff hediff, Pawn ___pawn)
        //    {
        //        CompMeeseeksMemory compMeeseeksMemory = ___pawn.GetComp<CompMeeseeksMemory>();

        //        if (compMeeseeksMemory != null)
        //        {
        //            Log.Warning("Meeseeks downed somehow: " + hediff.def.defName);
        //        }
        //    }
        //}
    }
}
