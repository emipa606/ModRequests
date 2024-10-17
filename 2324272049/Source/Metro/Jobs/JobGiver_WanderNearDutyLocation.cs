using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
	public class JobGiver_WanderNearDutyLocation : JobGiver_Wander
	{
		public JobGiver_WanderNearDutyLocation()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
			wanderDestValidator = (Pawn pawn, IntVec3 loc, IntVec3 root) => MetroUtils.IsNightNow(pawn.Map) || pawn.GetHiveCells().Contains(loc);
		}
		public override IntVec3 GetWanderRoot(Pawn pawn)
		{
			var cell = WanderUtility.BestCloseWanderRoot(pawn.mindState.duty.focus.Cell, pawn);
			if (!cell.IsValid || !pawn.GetHiveCells().Contains(cell))
            {
				cell = BestCloseWanderRoot2(pawn);
			}
			//Log.Message(pawn + " - gets GetWanderRoot: " + cell, true);
			return cell;
		}
		public IntVec3 BestCloseWanderRoot2(Pawn pawn)
		{
			foreach (var intVec in GenRadial.RadialCellsAround(pawn.Position, 50f, true).InRandomOrder())
			{
				if (intVec.InBounds(pawn.Map) && (MetroUtils.IsNightNow(pawn.Map) || pawn.GetHiveCells().Contains(intVec)) &&
					intVec.Walkable(pawn.Map) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Some))
				{
					return intVec;
				}
			}
			return pawn.Position;
		}
	}
}
