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
    public class MeeseeksJobSelector_Construction : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.IsConstruction;
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Job job = null;

            ConstructionStatus status = jobTarget.TargetConstructionStatus(meeseeks.MapHeld);

            Logger.MessageFormat(this, "Checking for blocker, construction status: {0}", status);

            if (status == ConstructionStatus.None)
            {
                BuildableDef buildableDef = jobTarget.BuildableDef;

                if (buildableDef != null && GenConstruct.PlaceBlueprintForBuild(buildableDef, jobTarget.Cell, meeseeks.MapHeld, jobTarget.blueprintRotation, meeseeks.Faction, jobTarget.blueprintStuff) != null)
                    status = ConstructionStatus.InProgress;
            }

            if (status == ConstructionStatus.Blocked)
            {
                job = GetDeconstructingJob(meeseeks, jobTarget, meeseeks.MapHeld);
                if (job == null)
                    jobAvailabilty = JobAvailability.Delayed;
                else
                    jobAvailabilty = JobAvailability.Available;
            }
            else if (status == ConstructionStatus.InProgress)
            {
                job = ScanForJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty, true);
                if (job == null)
                    jobAvailabilty = JobAvailability.Delayed;
                else
                    jobAvailabilty = JobAvailability.Available;
            }
            else if (status == ConstructionStatus.Complete)
            {
                jobAvailabilty = JobAvailability.Complete;
            }

            return job;
        }

        private Job GetDeconstructingJob(Pawn meeseeks, SavedTargetInfo jobTarget, Map map)
        {
            BuildableDef buildableDef = jobTarget.BuildableDef;
            if (buildableDef == null)
                return null;

            CellRect cellRect = GenAdj.OccupiedRect(jobTarget.Cell, jobTarget.blueprintRotation, buildableDef.Size);
            foreach (IntVec3 cell in cellRect)
            {
                foreach (Thing thing in cell.GetThingList(map))
                {
                    if (!GenConstruct.CanPlaceBlueprintOver(buildableDef, thing.def))
                        return JobMaker.MakeJob(JobDefOf.Deconstruct, thing);
                }
            }

            return null;
        }
    }
}
