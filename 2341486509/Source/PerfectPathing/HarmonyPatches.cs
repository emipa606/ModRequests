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
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("JP_PP: v.1.4.0 Loading..");
            var harmony = new Harmony("PerfectPathingPatch");

            //Heuristic Strength Patch
            MethodInfo determineHeuristicStrength = AccessTools.Method("Verse.AI.PathFinder:DetermineHeuristicStrength");
            harmony.Patch(determineHeuristicStrength, postfix: new HarmonyMethod(typeof(HeuristicStrengthPatch).GetMethod("Patch_DetermineHeuristicStrength")));

            Type[] arguments = new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning) };
            MethodInfo findPath = AccessTools.Method("Verse.AI.PathFinder:FindPath", arguments);

            if (Settings.lightPatch || Settings.weatherPatch)
            {
                harmony.Patch(findPath, transpiler: new HarmonyMethod(typeof(EnvironmentPatch).GetMethod("Patch_FindPath")));
                Log.Message("JP_PP: Environment Patch Enabled");
            }

            if (Settings.dirtAvoidance)
            {
                harmony.Patch(findPath, transpiler: new HarmonyMethod(typeof(DirtAvoid).GetMethod("Patch_FindPath")));
                Log.Message("JP_PP: Dirt Avoidance Enabled");
            }

            if (AccessTools.TypeByName("CleanPathfinding.Mod_CleanPathfinding") != null)
            {
                //Log.Message("JP_PP: Clean Pathfinding found");
            }
        }
    }
}

