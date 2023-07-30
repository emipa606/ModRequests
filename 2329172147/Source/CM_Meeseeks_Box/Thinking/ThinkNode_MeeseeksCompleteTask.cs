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
    public enum JobAvailability
    {
        Invalid,
        Delayed,
        Complete,
        Available
    }

    public class ThinkNode_MeeseeksCompleteTask : ThinkNode
    {
        private MeeseeksJobSelector defaultJobSelector = new MeeseeksJobSelector();
        private List<MeeseeksJobSelector> jobSelectors = new List<MeeseeksJobSelector>();

        public ThinkNode_MeeseeksCompleteTask()
        {
            jobSelectors = new List<MeeseeksJobSelector>();

            jobSelectors.Add(new MeeseeksJobSelector_Guard());

            jobSelectors.Add(new MeeseeksJobSelector_BuildRoof());
            jobSelectors.Add(new MeeseeksJobSelector_DoBill());
            jobSelectors.Add(new MeeseeksJobSelector_Construction());
            jobSelectors.Add(new MeeseeksJobSelector_PressButton());
            jobSelectors.Add(new MeeseeksJobSelector_RemoveRoof());
            jobSelectors.Add(new MeeseeksJobSelector_Tame());
            jobSelectors.Add(new MeeseeksJobSelector_Train());
        }
        
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_QueuedJob obj = (ThinkNode_QueuedJob)base.DeepCopy(resolve);
            return obj;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            Logger.MessageFormat(this, "Checking for saved job on Meeseeks.");

            CompMeeseeksMemory compMeeseeksMemory = pawn.GetComp<CompMeeseeksMemory>();

            if (compMeeseeksMemory != null && compMeeseeksMemory.GivenTask)
            {
                SavedJob savedJob = compMeeseeksMemory.savedJob;

                if (savedJob == null || CompMeeseeksMemory.noContinueJobs.Contains(savedJob.def))
                    return ThinkResult.NoJob;

                Job nextJob = GetNextJob(pawn, compMeeseeksMemory);

                if (nextJob == null && compMeeseeksMemory.jobTargets.Count == 0)
                    nextJob = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_EmbraceTheVoid);

                if (nextJob != null)
                    return new ThinkResult(nextJob, this, JobTag.MiscWork, fromQueue: false);
            }

            return ThinkResult.NoJob;
        }

        private Job GetNextJob(Pawn meeseeks, CompMeeseeksMemory memory)
        {
            Job nextJob = null;

            SavedJob savedJob = memory.savedJob;
            if (savedJob == null)
            {
                Logger.MessageFormat(this, "No saved job...");
                return null;
            }

            if (memory.jobStuck)
            {
                Logger.MessageFormat(this, "Wait a tick...");
                return JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture, 1);
            }

            //Logger.MessageFormat(this, "Job target count: {0}", memory.jobTargets.Count);

            Map map = meeseeks.MapHeld;

            List<SavedTargetInfo> delayedTargets = new List<SavedTargetInfo>();
            MeeseeksJobSelector jobSelector = defaultJobSelector;

            foreach (MeeseeksJobSelector eachJobSelector in jobSelectors)
            {
                if (eachJobSelector.UseForJob(meeseeks, memory, savedJob))
                {
                    jobSelector = eachJobSelector;
                    break;
                }
            }

            try
            {
                jobSelector.SortAndFilterJobTargets(meeseeks, memory, savedJob);

                while (memory.jobTargets.Count > 0 && nextJob == null)
                { 
                    JobAvailability jobAvailabilty = JobAvailability.Invalid;
                    SavedTargetInfo jobTarget = memory.jobTargets.FirstOrDefault();

                    if (jobTarget == null || !jobTarget.IsValid)
                    {
                        Logger.WarningFormat(this, "Invalid or null target in queue");
                        memory.jobTargets.RemoveAt(0);
                        continue;
                    }

                    nextJob = jobSelector.GetJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);
                    //Logger.MessageFormat(this, "Job selector: {0}, result: {1}", jobSelector.GetType().ToString(), jobAvailabilty.ToString());

                    if (nextJob != null)
                    {
                        bool reservationsMade = nextJob.TryMakePreToilReservations(meeseeks, false);
                        if (!reservationsMade)
                        {
                            jobAvailabilty = JobAvailability.Delayed;
                            Logger.MessageFormat(this, "Delaying job for {0} because reservations could not be made.", jobTarget.ToString());

                            nextJob = null;
                        }
                    }

                    if (jobAvailabilty == JobAvailability.Delayed)
                    {
                        delayedTargets.Add(jobTarget);
                        memory.jobTargets.RemoveAt(0);
                    }
                    else if (nextJob == null)
                    {
                        memory.jobTargets.RemoveAt(0);
                    }
                }

                if (delayedTargets.Count > 0 && nextJob == null)
                {
                    nextJob = jobSelector.GetJobDelayed(meeseeks, memory, savedJob, delayedTargets[0]);
                }
            }
            finally
            {
                // Put delayed targets back on the target list
                memory.jobTargets.AddRange(delayedTargets);
            }
            return nextJob;
        }
    }
}
