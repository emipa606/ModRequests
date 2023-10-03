using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;

namespace ExecutionModByYuno
{
    public class JobGiver_WatchExecution : JobGiver_SpectateDutySpectateRect
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			bool flag = duty == null;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				IntVec3 cell;
				bool flag2 = !SpectatorCellFinder.TryFindSpectatorCellFor(pawn, duty.spectateRect, pawn.Map, out cell, duty.spectateRectAllowedSides, 1, null);
				if (flag2)
				{
					result = null;
				}
				else
				{
					IntVec3 centerCell = duty.spectateRect.CenterCell;
					Building edifice = cell.GetEdifice(pawn.Map);
					bool flag3 = edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable && pawn.CanReserve(edifice, 1, -1, null, false);
					if (flag3)
					{
						result = new Job(JobDefOf.WatchExecution, edifice, centerCell);
					}
					else
					{
						result = new Job(JobDefOf.WatchExecution, cell, centerCell);
					}
				}
			}
			return result;
		}

	}
}
