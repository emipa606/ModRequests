using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection.Emit;

namespace PerfectPathing
{
    class HeuristicStrengthPatch
    {
        public static void Patch_DetermineHeuristicStrength(ref float __result, Pawn pawn, IntVec3 start, LocalTargetInfo dest)
        {
            if (Settings.heuristicPatch && pawn != null && !pawn.RaceProps.Animal)
                __result = 1f;
        }
    }
}
