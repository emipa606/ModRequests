using RimWorld;
using System;
using Verse;

namespace RadWorld
{
	public class GenStep_FindLocationUnderground : GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 820815231;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			DeepProfiler.Start("RebuildAllRegions");
			map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			MapGenerator.PlayerStartSpot = CellFinderLoose.RandomCellWith((IntVec3 x) => x.Walkable(map) && x.GetRoof(map) != RoofDefOf.RoofRockThick, map);
		}
	}
}

