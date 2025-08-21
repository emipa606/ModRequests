using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pyronomaniac
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            var harmony = new Harmony("Pyronomaniac.HarmonyPatches");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    public static class Patch_JobGiver_FireStartingSpree_TryGiveJob
    {
        // Specify the target method using AccessTools
        static MethodBase TargetMethod()
        {
            return AccessTools.Method("RimWorld.JobGiver_FireStartingSpree:TryGiveJob");
        }

        // Prefix method to run before the original TryGiveJob logic
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            // Search for a PyroFirePit to move to
            Thing pyroFirePit = TryFindPyroFirePitToIgnite(pawn);
            if (pyroFirePit != null)
            {
                // Get perimeter cells around the PyroFirePit
                List<IntVec3> perimeterCells = GetPerimeterCells(pyroFirePit.Position);

                // Choose a random perimeter cell to move to
                IntVec3 targetCell = perimeterCells.RandomElement();

                // Assign a job to wait/wander at the target perimeter cell
                __result = JobMaker.MakeJob(JobDefOf.Goto, targetCell);
                __result.expiryInterval = 500; // Reevaluate every 500 ticks
                return false; // Skip the original method
            }

            // If no PyroFirePit is found, allow the original behavior to execute
            return true; // Continue with the original TryGiveJob method
        }

        // Get perimeter cells around a 1x1 PyroFirePit
        private static List<IntVec3> GetPerimeterCells(IntVec3 center)
        {
            List<IntVec3> perimeterCells = new List<IntVec3>();
            foreach (IntVec3 offset in GenAdj.CardinalDirectionsAndInside)
            {
                IntVec3 cell = center + offset;
                if (cell != center) // Exclude the center cell
                {
                    perimeterCells.Add(cell);
                }
            }
            return perimeterCells;
        }

        // Method to find a nearby PyroFirePit
        private static Thing TryFindPyroFirePitToIgnite(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing t)
            {
                return t.def.defName == "PyroFirePit" && t.FlammableNow && !t.IsBurning() && pawn.CanReserve(t);
            };

            // Search for a PyroFirePit within a 50-tile radius
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("PyroFirePit")),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                50f,
                validator);
        }

        // Check if the pawn is near a PyroFirePit
        private static bool IsPawnNearPyroFirePit(Pawn pawn)
        {
            foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("PyroFirePit")))
            {
                if (thing.Position.InHorDistOf(pawn.Position, 5f))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
