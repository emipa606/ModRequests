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
    public class MeeseeksJobSelector_PressButton : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.UsesWorkGiver<WorkGiver_PressButton>();
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Job job = null;

            if (jobTarget != null && jobTarget.HasThing && !jobTarget.ThingDestroyed)
            {
                CompHasButton hasButton = jobTarget.Thing.TryGetComp<CompHasButton>();
                
                if (!hasButton.WantsPress)
                {
                    jobAvailabilty = JobAvailability.Complete;
                }
            }
            else
            {
                Logger.WarningFormat(this, "Unable to get scanner or target for job.");
            }

            if (job != null)
                jobAvailabilty = JobAvailability.Available;

            return job;
        }
    }
}
