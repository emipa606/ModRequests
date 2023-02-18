using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Crystosentient.Biome
{
    public class GenStep_CrystalFromGrid : RimWorld.GenStep_RocksFromGrid
    {
        #region Overrides
        private class RoofThreshold
        {
            public RoofDef roofDef;

            public float minGridVal;
        }

        private float maxMineableValue = float.MaxValue;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!map.TileInfo.WaterCovered)
            {
                map.regionAndRoomUpdater.Enabled = false;
                float num = 0.7f;
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
                foreach (IntVec3 allCell in map.AllCells)
                {
                    float num2 = elevation[allCell];
                    if (num2 > num)
                    {
                        if (caves[allCell] <= 0f)
                        {
                            ThingDef rockdef;
                            if (map.Biome.defName == "GDCRYST_BIOME_GemGardenAmethyst")
                            {
                                rockdef = DefDatabase<ThingDef>.GetNamed("GDCRYST_BUILDABLE_WallAmethystRough");
                            }

                            else
                            {
                                rockdef = RockDefAt(allCell);
                            }
                            GenSpawn.Spawn(rockdef, allCell, map);
                        }
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (num2 > list[i].minGridVal)
                            {
                                map.roofGrid.SetRoof(allCell, list[i].roofDef);
                                break;
                            }
                        }
                    }
                }
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
            return c.Roofed(map) && c.GetRoof(map).isNatural;
        }
        #endregion
    }
}