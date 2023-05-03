using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Mashed_BlackPlague
{
    class JobGiver_TouchVessel : ThinkNode_JobGiver
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_TouchVessel mentalState_TouchVessel = pawn.MentalState as MentalState_TouchVessel;
			if (mentalState_TouchVessel == null || mentalState_TouchVessel.vessel == null || mentalState_TouchVessel.alreadyTouchedVessel)
			{
				return null;
			}
			Thing vessel = mentalState_TouchVessel.vessel;

			if (!pawn.CanReserveAndReach(vessel, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.UseArtifact, vessel);
			job.count = 1;
			return job;
		}
	}
}
