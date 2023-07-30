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
    public class MeeseeksJobSelector_RemoveRoof : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.UsesWorkGiver<WorkGiver_RemoveRoof>();
        }

        public override void SortAndFilterJobTargets(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            Map map = meeseeks.MapHeld;
            WorkGiver_RemoveRoof scanner = savedJob.workGiverDef.Worker as WorkGiver_RemoveRoof;

            SavedTargetInfo bestTarget = null;
            float bestPriority = float.MinValue;
            float bestDistanceSquared = float.MaxValue;

            for (int i = memory.jobTargets.Count - 1; i >= 0; --i)
            {
                SavedTargetInfo target = memory.jobTargets[i];
                if (!target.Cell.Roofed(map))
                {
                    CollectNewTargets(meeseeks, memory, target.Cell, map);
                    memory.jobTargets.RemoveAt(i);
                }
            }

            foreach (SavedTargetInfo target in memory.jobTargets)
            {
                if (meeseeks.CanReach(target.Cell, scanner.PathEndMode, Danger.Deadly) && meeseeks.CanReserve(target.Cell, 1, -1, ReservationLayerDefOf.Ceiling, false))
                {
                    float priority = scanner.GetPriority(meeseeks, target.Cell);
                    float distanceSquared = (target.Cell - meeseeks.Position).LengthHorizontalSquared;
                    
                    if (priority > bestPriority || (priority == bestPriority && distanceSquared < bestDistanceSquared))
                    {
                        bestTarget = target;
                        bestPriority = priority;
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
                if (nearCell.InBounds(map) && nearCell.Roofed(map) && map.areaManager.NoRoof[nearCell])
                {
                    memory.AddJobTarget(nearCell);
                }
            }
        }
    }
}
