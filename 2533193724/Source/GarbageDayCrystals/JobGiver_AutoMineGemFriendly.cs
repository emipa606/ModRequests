using System;
using Crystosentient.Dictionary;
using RimWorld;
using Verse;
using Verse.AI;

namespace Crystosentient
{
	// Token: 0x0200000D RID: 13
	public class JobGiver_AutoMineGemFriendly : ThinkNode_JobGiver
	{
		// Token: 0x06000056 RID: 86 RVA: 0x00003D24 File Offset: 0x00001F24
		protected override Job TryGiveJob(Pawn pawn)
		{
			Region region = pawn.GetRegion(RegionType.Normal | RegionType.Portal);
			bool flag = region == null;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				for (int i = 0; i < 40; i++)
				{
					IntVec3 randomCell = region.RandomCell;
					for (int j = 0; j < 4; j++)
					{
						IntVec3 c = randomCell + GenAdj.CardinalDirections[j];
						bool flag2 = c.InBounds(pawn.Map);
						if (flag2)
						{
							Building mineable = c.GetFirstMineable(pawn.Map);
							bool flag3 = mineable != null && (mineable.def.size == IntVec2.One && mineable.def != DefOfThing.GDCRYST_BUILDABLE_WallAmethystRough && pawn.CanReserve(mineable, 1, -1, null, false));
							if (flag3)
							{
								Job job = JobMaker.MakeJob(JobDefOf.Mine,mineable);
								job.ignoreDesignations = true;
								return job;
							}

							Building building = c.GetFirstBuilding(pawn.Map);
							bool flag4 = building != null && (building.def != DefOfThing.GDCRYST_BUILDABLE_WallAmethystRough && building.def != DefOfThing.GDCRYST_BUILDABLE_WallAmethystSmooth && pawn.CanReserve(building, 1, -1, null, false));
							if (flag4)
							{
								Job job = JobMaker.MakeJob(JobDefOf.Deconstruct, building);
								job.ignoreDesignations = true;
								return job;
							}

							//TerrainDef terrain = GridsUtility.GetTerrain(c, pawn.Map);
							//bool flag5 = terrain.layerable && pawn.CanReserve(c, 1, -1, null, false);
							//if (flag5)
							//{
							//	Job job = JobMaker.MakeJob(JobDefOf.RemoveFloor, c);
							//	job.ignoreDesignations = true;
							//	return job;
					     	//	}
						}
					}
				}
				result = null;
			}
			return result;
		}
	}
}
