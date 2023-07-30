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
    public class MeeseeksJobSelector_Guard : MeeseeksJobSelector
    {
        public override bool WaitBeforeRepeat => false;

        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return memory.guardPosition.IsValid;
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            bool wait = true;

            IntVec3 guardPosition = memory.guardPosition;

            if (memory.guardPosition.IsValid && meeseeks.Position != guardPosition)
            {
                guardPosition = RCellFinder.BestOrderedGotoDestNear(memory.guardPosition, meeseeks);
                if (guardPosition.IsValid && meeseeks.Position != guardPosition)
                {
                    wait = false;
                }
            }

            Job job = null;

            if (wait)
            {
                Logger.MessageFormat(this, "Wait");
                job = JobMaker.MakeJob(JobDefOf.Wait_Combat, memory.guardPosition);
                job.expiryInterval = 600;
            }
            else
            {
                Logger.MessageFormat(this, "Goto spot");
                job = JobMaker.MakeJob(JobDefOf.Goto, guardPosition);
                job.expiryInterval = 120;
            }

            return job;
        }
    }
}
