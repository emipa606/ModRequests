using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace CM_Meeseeks_Box
{
    public class MeeseeksJobSelector_BuildRoof : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.UsesWorkGiver<WorkGiver_BuildRoof>();
        }

        public override void SortAndFilterJobTargets(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            Map map = meeseeks.MapHeld;
            WorkGiver_BuildRoof scanner = savedJob.workGiverDef.Worker as WorkGiver_BuildRoof;

            SavedTargetInfo bestTarget = null;
            float bestDistanceSquared = float.MaxValue;

            for (int i = memory.jobTargets.Count - 1; i >= 0; --i)
            {
                SavedTargetInfo target = memory.jobTargets[i];
                if (target.Cell.Roofed(map))
                {
                    CollectNewTargets(meeseeks, memory, target.Cell, map);
                    memory.jobTargets.RemoveAt(i);
                }
            }

            foreach (SavedTargetInfo target in memory.jobTargets)
            {
                if (meeseeks.CanReach(target.Cell, scanner.PathEndMode, Danger.Deadly) && meeseeks.CanReserve(target.Cell, 1, -1, ReservationLayerDefOf.Ceiling, false))
                {
                    float distanceSquared = (target.Cell - meeseeks.Position).LengthHorizontalSquared;

                    if (distanceSquared < bestDistanceSquared)
                    {
                        bestTarget = target;
                        bestDistanceSquared = distanceSquared;
                    }
                }
            }

            if (bestTarget != null)
            {
                memory.jobTargets.Remove(bestTarget);
                memory.jobTargets.Insert(0, bestTarget);
            }
        }

        private void CollectNewTargets(Pawn meeseeks, CompMeeseeksMemory memory, IntVec3 cell, Map map)
        {
            foreach (IntVec3 nearCell in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (nearCell.InBounds(map) && !nearCell.Roofed(map) && map.areaManager.BuildRoof[nearCell])
                {
                    memory.AddJobTarget(nearCell);
                }
            }
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Map map = meeseeks.MapHeld;
            Job job = ScanForJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);

            // Mark them now because building a roof will cover most if them, and we will need a chance to check their neighbors
            CollectNewTargets(meeseeks, memory, jobTarget.Cell, map);

            if (job != null)
                jobAvailabilty = JobAvailability.Available;

            return job;
        }
    }
}
