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
    public class MeeseeksJobSelector
    {
        public virtual bool WaitBeforeRepeat => true;

        public virtual bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return true;
        }

        public virtual void SortAndFilterJobTargets(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            memory.SortJobTargets();
        }

        public virtual Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            return ScanForJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);
        }

        public virtual Job GetJobDelayed(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget)
        {
            return null;
        }

        protected Job ScanForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty, bool scanAllThingsOnCell = false)
        {
            Job job = null;

            if (savedJob.workGiverDef != null && savedJob.workGiverDef.Worker != null)
            {
                WorkGiver_Scanner workGiverScanner = savedJob.workGiverDef.Worker as WorkGiver_Scanner;
                if (workGiverScanner != null)
                {
                    List<WorkGiver_Scanner> workGivers = WorkerDefUtility.GetCombinedWorkGiverScanners(workGiverScanner);

                    foreach (WorkGiver_Scanner scanner in workGivers)
                    {
                        job = this.GetJobOnTarget(meeseeks, jobTarget, scanner, scanAllThingsOnCell);

                        if (job != null)
                        {
                            if (memory.JobStuckRepeat(job))
                            {
                                Logger.ErrorFormat(this, "Stuck job detected and removed on {0}.", jobTarget);
                                jobAvailabilty = JobAvailability.Delayed;
                                job = null;
                            }
                            else
                            {
                                //Logger.MessageFormat(this, "Job WAS found for {0}.", scanner.def.defName);
                                jobAvailabilty = JobAvailability.Available;
                                return job;
                            }
                        }

                        //Logger.MessageFormat(this, "No {0} job found.", scanner.def.defName);
                    }
                }
                else
                {
                    Logger.WarningFormat(this, "No work scanner");
                }
            }
            else
            {
                Logger.WarningFormat(this, "Missing saved job workGiverDef or Worker for savedJob: {0}", savedJob.def.defName);
            }

            return job;
        }

        protected Job GetJobOnTarget(Pawn meeseeks, SavedTargetInfo targetInfo, WorkGiver_Scanner workGiverScanner, bool scanAllThingsOnCell = false)
        {
            Job job = null;

            try
            {
                DesignatorUtility.ForceAllDesignationsOnCell(targetInfo.Cell, meeseeks.MapHeld);

                if (targetInfo.HasThing && !targetInfo.ThingDestroyed)
                {
                    // Special case for uninstall, the workgiver doesn't check to see if its already uninstalled
                    if (workGiverScanner as WorkGiver_Uninstall != null && !targetInfo.Thing.Spawned)
                    {
                        Logger.WarningFormat(this, "Target is inside {0}", targetInfo.Thing.ParentHolder);
                        DesignatorUtility.RestoreDesignationsOnCell(targetInfo.Cell, meeseeks.MapHeld);
                        return null;
                    }

                    // Have to try-catch all these damn things because other mods arent't doing null checks :(
                    try
                    {
                        //Logger.MessageFormat(this, "Checking {0} for job on {1}", workGiverScanner, targetInfo.Thing);
                        if (workGiverScanner.HasJobOnThing(meeseeks, targetInfo.Thing, true))
                        {
                            //Logger.MessageFormat(this, "Getting {0} for job on {1}", workGiverScanner, targetInfo.Thing);
                            job = workGiverScanner.JobOnThing(meeseeks, targetInfo.Thing, true);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {

                    }
                }
                else
                {
                    // Have to try-catch all these damn things because other mods arent't doing null checks :(
                    try
                    {
                        //Logger.MessageFormat(this, "Checking {0} for job on {1}", workGiverScanner, targetInfo.Cell);
                        if (job == null && workGiverScanner.HasJobOnCell(meeseeks, targetInfo.Cell, true))
                            job = workGiverScanner.JobOnCell(meeseeks, targetInfo.Cell, true);
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {

                    }
                }

                if (job == null && scanAllThingsOnCell)
                {
                    var thingsAtCell = meeseeks.MapHeld.thingGrid.ThingsAt(targetInfo.Cell);
                    foreach (Thing thing in thingsAtCell)
                    {
                        //Logger.MessageFormat(this, "Checking {0} for {1}.", thing, workGiverScanner.def.defName);

                        // Have to try-catch all these damn things because other mods arent't doing null checks :(
                        try
                        {
                            Logger.MessageFormat(this, "Checking {0} for job on {1}", workGiverScanner, thing);
                            if (workGiverScanner.HasJobOnThing(meeseeks, thing, true))
                                job = workGiverScanner.JobOnThing(meeseeks, thing, true);
                        }
                        catch (Exception e)
                        {

                        }
                        finally
                        {

                        }

                        if (job != null)
                            break;
                    }
                }
            }
            finally
            {
                DesignatorUtility.RestoreDesignationsOnCell(targetInfo.Cell, meeseeks.MapHeld);
            }

            return job;
        }

        protected Job ExitMapJob(Pawn meeseeks)
        {
            IntVec3 destination;

            if (!RCellFinder.TryFindBestExitSpot(meeseeks, out destination, TraverseMode.ByPawn))
                return null;

            Job job2 = JobMaker.MakeJob(JobDefOf.Goto, destination);
            job2.exitMapOnArrival = true;
            job2.failIfCantJoinOrCreateCaravan = false;
            job2.locomotionUrgency = PawnUtility.ResolveLocomotion(meeseeks, LocomotionUrgency.Walk, LocomotionUrgency.Jog);
            //job2.expiryInterval = jobMaxDuration;
            //job2.canBash = canBash;
            return job2;
        }
    }
}
