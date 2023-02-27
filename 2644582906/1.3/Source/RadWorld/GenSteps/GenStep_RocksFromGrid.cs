using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RadWorld
{
	public class GenStep_RocksFromGridUnderground : GenStep
	{
		private class RoofThreshold
		{
			public RoofDef roofDef;

			public float minGridVal;
		}

		private float maxMineableValue = float.MaxValue;

		private const int MinRoofedCellsPerGroup = 20;

		public override int SeedPart => 1182952823;

		public static ThingDef RockDefAt(IntVec3 c)
		{
			ThingDef thingDef = null;
			float num = -999999f;
			for (int i = 0; i < RockNoises.rockNoises.Count; i++)
			{
				float value = RockNoises.rockNoises[i].noise.GetValue(c);
				if (value > num)
				{
					thingDef = RockNoises.rockNoises[i].rockDef;
					num = value;
				}
			}
			if (thingDef == null)
			{
				Log.ErrorOnce("Did not get rock def to generate at " + c, 50812);
				thingDef = ThingDefOf.Sandstone;
			}
			return thingDef;
		}


		public override void Generate(Map map, GenStepParams parms)
		{
			if (!map.TileInfo.WaterCovered)
			{
				map.regionAndRoomUpdater.Enabled = false;
				float num = 0.2f;
				List<RoofThreshold> list = new List<RoofThreshold>();
				RoofThreshold roofThreshold = new RoofThreshold();
				roofThreshold.roofDef = RoofDefOf.RoofRockThick;
				roofThreshold.minGridVal = num * 1.14f;
				list.Add(roofThreshold);
				RoofThreshold roofThreshold2 = new RoofThreshold();
				roofThreshold2.roofDef = RoofDefOf.RoofRockThin;
				roofThreshold2.minGridVal = num * 1.04f;
				list.Add(roofThreshold2);
				MapGenFloatGrid elevation = MapGenerator.Elevation;
				MapGenFloatGrid caves = MapGenerator.Caves;

				//HashSet<IntVec3> rockCells = new HashSet<IntVec3>();
				//HashSet<IntVec3> openWalkableCells = new HashSet<IntVec3>();
				foreach (IntVec3 allCell in map.AllCells)
				{
					float num2 = elevation[allCell];
					if (caves[allCell] <= 0f)
					{
						GenSpawn.Spawn(RockDefAt(allCell), allCell, map);
						//rockCells.Add(allCell);
					}
					map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThin);
				}

				//var desiredThinRoovesCount = Rand.Range(4f, 7f);
				//float totalWalkableCellCount = openWalkableCells.Count;
				//var thinRooveSpawnChance = desiredThinRoovesCount / totalWalkableCellCount;
				//
				//foreach (IntVec3 allCell in map.AllCells)
				//{
				//	if (openWalkableCells.Contains(allCell) && Rand.Chance(thinRooveSpawnChance))
				//	{
				//	}
				//	else
				//	{
				//		map.roofGrid.SetRoof(allCell, RoofDefOf.RoofRockThick);
				//	}
				//}
				BoolGrid visited = new BoolGrid(map);
				List<IntVec3> toRemove = new List<IntVec3>();
				foreach (IntVec3 allCell2 in map.AllCells)
				{
					if (!visited[allCell2] && IsNaturalRoofAt(allCell2, map))
					{
						toRemove.Clear();
						map.floodFiller.FloodFill(allCell2, (IntVec3 x) => IsNaturalRoofAt(x, map), delegate (IntVec3 x)
						{
							visited[x] = true;
							toRemove.Add(x);
						});
						if (toRemove.Count < 20)
						{
							for (int j = 0; j < toRemove.Count; j++)
							{
								map.roofGrid.SetRoof(toRemove[j], null);
							}
						}
					}
				}
				GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
				genStep_ScatterLumpsMineable.maxValue = maxMineableValue;
				float num3 = 10f;
				switch (Find.WorldGrid[map.Tile].hilliness)
				{
					case Hilliness.Flat:
						num3 = 4f;
						break;
					case Hilliness.SmallHills:
						num3 = 8f;
						break;
					case Hilliness.LargeHills:
						num3 = 11f;
						break;
					case Hilliness.Mountainous:
						num3 = 15f;
						break;
					case Hilliness.Impassable:
						num3 = 16f;
						break;
				}
				genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
				genStep_ScatterLumpsMineable.Generate(map, parms);
				map.regionAndRoomUpdater.Enabled = true;
			}
		}

		private bool IsNaturalRoofAt(IntVec3 c, Map map)
		{
			if (c.Roofed(map))
			{
				return c.GetRoof(map).isNatural;
			}
			return false;
		}
	}
}

