using System;
using Verse;
using HLVRMonsters;
using RimWorld;

namespace HLVRMonsters
{
	// Token: 0x02000A39 RID: 2617
	public static class AntlionInfestationUtility
	{
		// Token: 0x06003E99 RID: 16025 RVA: 0x0014A91C File Offset: 0x00148B1C
		public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null)
		{
			IntVec3 loc;
			if (!InfestationCellFinder.TryFindCell(out loc, map))
			{
				if (!spawnAnywhereIfNoGoodCell)
				{
					return null;
				}
				if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate (IntVec3 x)
				{
					if (!x.Standable(map) || x.Fogged(map))
					{
						return false;
					}
					bool result = false;
					int num = GenRadial.NumCellsInRadius(3f);
					for (int j = 0; j < num; j++)
					{
						IntVec3 c = x + GenRadial.RadialPattern[j];
						if (c.InBounds(map))
						{
							RoofDef roof = c.GetRoof(map);
							if (roof != null && roof.isThickRoof)
							{
								result = true;
								break;
							}
						}
					}
					return result;
				}, map, out loc))
				{
					return null;
				}
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(HLVRDefOf.AntlionTunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
			QuestUtility.AddQuestTag(thing, questTag);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, HLVRDefOf.AntlionHive, HLVRDefOf.AntlionHive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, true);
				if (loc.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(HLVRDefOf.AntlionTunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
	}
}
