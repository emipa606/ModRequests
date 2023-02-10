using RimWorld;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SearchAndDestroy
{
    class JobGiver_GoWithinRangeOfHostile : JobGiver_AIGotoNearestHostile
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			var job = base.TryGiveJob(pawn);
			if(job != null)
            {
				job.expiryInterval = 30;
            }
			return job;
		}
	}
}
