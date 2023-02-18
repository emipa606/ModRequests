using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace Crystosentient.Biome
{
    public class GenStep_CrystalTerrain : RimWorld.GenStep_Terrain
    {
        #region Overrides

        private struct GRLT_Entry
        {
            public float bestDistance;

            public IntVec3 bestNode;
        }
        private static bool debug_WarnedMissingTerrain;

        public override int SeedPart => 262606459;


        public override void Generate(Map map, GenStepParams parms)
        {
            BeachMaker.Init(map);
            RiverMaker riverMaker = GenerateRiver(map);
            List<IntVec3> list = new List<IntVec3>();
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            MapGenFloatGrid fertility = MapGenerator.Fertility;
            MapGenFloatGrid caves = MapGenerator.Caves;
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (IntVec3 allCell in map.AllCells)
            {
                Building edifice = allCell.GetEdifice(map);
                TerrainDef terrainDef = null;
                terrainDef = (((edifice == null || edifice.def.Fillage != FillCategory.Full) && !(caves[allCell] > 0f)) ? TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], riverMaker, false) : TerrainFrom(allCell, map, elevation[allCell], fertility[allCell], riverMaker, true));
                if (terrainDef.IsRiver && edifice != null)
                {
                    list.Add(edifice.Position);
                    edifice.Destroy();
                }
                if (map.Biome.defName == "GDCRYST_BIOME_GemGardenAmethyst")
                {
                    if (terrainDef.defName.Contains("Rough"))
                    {
                        terrainDef = DefDatabase<TerrainDef>.GetNamed("GDCRYST_BUILDABLE_FloorAmethystRough");
                    }
                    if (terrainDef.defName == "Sand") /// To replace the force generated core-sand which results from a lack of building affordances (i think is the cause)
                    {
                        terrainDef = DefDatabase<TerrainDef>.GetNamed("GDCRYST_BUILDABLE_FloorAmethystSand");
                    }
                    else if (terrainDef.defName == "Gravel")
                    {
                        terrainDef = DefDatabase<TerrainDef>.GetNamed("GDCRYST_BUILDABLE_FloorAmethystDunes");
                    }
                }
                terrainGrid.SetTerrain(allCell, terrainDef);
            }
            riverMaker?.ValidatePassage(map);
            RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
            BeachMaker.Cleanup();
            foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers)
            {
                terrainPatchMaker.Cleanup();
            }
        }

        private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
        {
            TerrainDef terrainDef = null;
            if (river != null)
            {
                terrainDef = river.TerrainAt(c, true);
            }
            if (terrainDef == null && preferSolid)
            {
                return RimWorld.GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
            }
            TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
            if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
            {
                return terrainDef2;
            }
            if (terrainDef != null && terrainDef.IsRiver)
            {
                return terrainDef;
            }
            if (terrainDef2 != null)
            {
                return terrainDef2;
            }
            if (terrainDef != null)
            {
                return terrainDef;
            }
            for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
            {
                terrainDef2 = map.Biome.terrainPatchMakers[i].TerrainAt(c, map, fertility);
                if (terrainDef2 != null)
                {
                    return terrainDef2;
                }
            }
            if (elevation > 0.55f && elevation < 0.61f)
            {
                return TerrainDefOf.Gravel;
            }
            if (elevation >= 0.61f)
            {
                return RimWorld.GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
            }
            terrainDef2 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
            if (terrainDef2 != null)
            {
                return terrainDef2;
            }
            if (!debug_WarnedMissingTerrain)
            {
                Log.Error("No terrain found in biome " + map.Biome.defName + " for elevation=" + elevation + ", fertility=" + fertility);
                debug_WarnedMissingTerrain = true;
            }
            return TerrainDefOf.Sand;
        }

        private void UpdateRiverAnchorEntry(Dictionary<int, GRLT_Entry> entries, IntVec3 center, int entryId, float zValue)
        {
            float num = zValue - (float)entryId;
            if (num > 2f)
            {
                return;
            }
            if (entries.ContainsKey(entryId))
            {
                GRLT_Entry gRLT_Entry = entries[entryId];
                if (!(gRLT_Entry.bestDistance > num))
                {
                    return;
                }
            }
            entries[entryId] = new GRLT_Entry
            {
                bestDistance = num,
                bestNode = center
            };
        }

        private RiverMaker GenerateRiver(Map map)
        {
            Tile tile = Find.WorldGrid[map.Tile];
            List<Tile.RiverLink> rivers = tile.Rivers;
            if (rivers == null || rivers.Count == 0)
            {
                return null;
            }
            WorldGrid worldGrid = Find.WorldGrid;
            int tile2 = map.Tile;
            Tile.RiverLink riverLink = (from rl in rivers
                                        orderby -rl.river.degradeThreshold
                                        select rl).First();
            float num = worldGrid.GetHeadingFromTo(tile2, riverLink.neighbor);
            Rot4 a = Find.World.CoastDirectionAt(map.Tile);
            if (a != Rot4.Invalid)
            {
                num = a.AsAngle + (float)Rand.RangeInclusive(-30, 30);
            }
            float num2 = Rand.Range(0.3f, 0.7f);
            IntVec3 size = map.Size;
            float x = num2 * (float)size.x;
            float num3 = Rand.Range(0.3f, 0.7f);
            IntVec3 size2 = map.Size;
            Vector3 vector = new Vector3(x, 0f, num3 * (float)size2.z);
            Vector3 center = vector;
            float angle = num;
            Tile.RiverLink riverLink2 = (from rl in rivers
                                         orderby -rl.river.degradeThreshold
                                         select rl).FirstOrDefault();
            RiverMaker riverMaker = new RiverMaker(center, angle, riverLink2.river);
            GenerateRiverLookupTexture(map, riverMaker);
            return riverMaker;
        }

        private void GenerateRiverLookupTexture(Map map, RiverMaker riverMaker)
        {
            int num = Mathf.CeilToInt((from rd in DefDatabase<RiverDef>.AllDefs
                                       select rd.widthOnMap / 2f + 8f).Max());
            int num2 = Mathf.Max(4, num) * 2;
            Dictionary<int, GRLT_Entry> dictionary = new Dictionary<int, GRLT_Entry>();
            Dictionary<int, GRLT_Entry> dictionary2 = new Dictionary<int, GRLT_Entry>();
            Dictionary<int, GRLT_Entry> dictionary3 = new Dictionary<int, GRLT_Entry>();
            int num3 = -num2;
            while (true)
            {
                int num4 = num3;
                IntVec3 size = map.Size;
                if (num4 >= size.z + num2)
                {
                    break;
                }
                int num5 = -num2;
                while (true)
                {
                    int num6 = num5;
                    IntVec3 size2 = map.Size;
                    if (num6 >= size2.x + num2)
                    {
                        break;
                    }
                    IntVec3 intVec = new IntVec3(num5, 0, num3);
                    Vector3 vector = riverMaker.WaterCoordinateAt(intVec);
                    int entryId = Mathf.FloorToInt(vector.z / 4f);
                    UpdateRiverAnchorEntry(dictionary, intVec, entryId, (vector.z + Mathf.Abs(vector.x)) / 4f);
                    UpdateRiverAnchorEntry(dictionary2, intVec, entryId, (vector.z + Mathf.Abs(vector.x - (float)num)) / 4f);
                    UpdateRiverAnchorEntry(dictionary3, intVec, entryId, (vector.z + Mathf.Abs(vector.x + (float)num)) / 4f);
                    num5++;
                }
                num3++;
            }
            int num7 = Mathf.Max(dictionary.Keys.Min(), dictionary2.Keys.Min(), dictionary3.Keys.Min());
            int num8 = Mathf.Min(dictionary.Keys.Max(), dictionary2.Keys.Max(), dictionary3.Keys.Max());
            for (int i = num7; i < num8; i++)
            {
                WaterInfo waterInfo = map.waterInfo;
                if (dictionary2.ContainsKey(i) && dictionary2.ContainsKey(i + 1))
                {
                    List<Vector3> riverDebugData = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry = dictionary2[i];
                    riverDebugData.Add(gRLT_Entry.bestNode.ToVector3Shifted());
                    List<Vector3> riverDebugData2 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry2 = dictionary2[i + 1];
                    riverDebugData2.Add(gRLT_Entry2.bestNode.ToVector3Shifted());
                }
                if (dictionary.ContainsKey(i) && dictionary.ContainsKey(i + 1))
                {
                    List<Vector3> riverDebugData3 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry3 = dictionary[i];
                    riverDebugData3.Add(gRLT_Entry3.bestNode.ToVector3Shifted());
                    List<Vector3> riverDebugData4 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry4 = dictionary[i + 1];
                    riverDebugData4.Add(gRLT_Entry4.bestNode.ToVector3Shifted());
                }
                if (dictionary3.ContainsKey(i) && dictionary3.ContainsKey(i + 1))
                {
                    List<Vector3> riverDebugData5 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry5 = dictionary3[i];
                    riverDebugData5.Add(gRLT_Entry5.bestNode.ToVector3Shifted());
                    List<Vector3> riverDebugData6 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry6 = dictionary3[i + 1];
                    riverDebugData6.Add(gRLT_Entry6.bestNode.ToVector3Shifted());
                }
                if (dictionary2.ContainsKey(i) && dictionary.ContainsKey(i))
                {
                    List<Vector3> riverDebugData7 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry7 = dictionary2[i];
                    riverDebugData7.Add(gRLT_Entry7.bestNode.ToVector3Shifted());
                    List<Vector3> riverDebugData8 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry8 = dictionary[i];
                    riverDebugData8.Add(gRLT_Entry8.bestNode.ToVector3Shifted());
                }
                if (dictionary.ContainsKey(i) && dictionary3.ContainsKey(i))
                {
                    List<Vector3> riverDebugData9 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry9 = dictionary[i];
                    riverDebugData9.Add(gRLT_Entry9.bestNode.ToVector3Shifted());
                    List<Vector3> riverDebugData10 = waterInfo.riverDebugData;
                    GRLT_Entry gRLT_Entry10 = dictionary3[i];
                    riverDebugData10.Add(gRLT_Entry10.bestNode.ToVector3Shifted());
                }
            }
            IntVec3 size3 = map.Size;
            int width = size3.x + 4;
            IntVec3 size4 = map.Size;
            CellRect cellRect = new CellRect(-2, -2, width, size4.z + 4);
            float[] array = new float[cellRect.Area * 2];
            int num9 = 0;
            for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
            {
                for (int k = cellRect.minX; k <= cellRect.maxX; k++)
                {
                    IntVec3 a = new IntVec3(k, 0, j);
                    bool flag = true;
                    for (int l = 0; l < GenAdj.AdjacentCellsAndInside.Length; l++)
                    {
                        if (riverMaker.TerrainAt(a + GenAdj.AdjacentCellsAndInside[l]) != null)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        Vector2 p = a.ToIntVec2.ToVector2();
                        int num10 = int.MinValue;
                        Vector2 vector2 = Vector2.zero;
                        for (int m = num7; m < num8; m++)
                        {
                            if (dictionary2.ContainsKey(m) && dictionary2.ContainsKey(m + 1) && dictionary.ContainsKey(m) && dictionary.ContainsKey(m + 1) && dictionary3.ContainsKey(m) && dictionary3.ContainsKey(m + 1))
                            {
                                GRLT_Entry gRLT_Entry11 = dictionary2[m];
                                Vector2 p2 = gRLT_Entry11.bestNode.ToIntVec2.ToVector2();
                                GRLT_Entry gRLT_Entry12 = dictionary2[m + 1];
                                Vector2 p3 = gRLT_Entry12.bestNode.ToIntVec2.ToVector2();
                                GRLT_Entry gRLT_Entry13 = dictionary[m];
                                Vector2 p4 = gRLT_Entry13.bestNode.ToIntVec2.ToVector2();
                                GRLT_Entry gRLT_Entry14 = dictionary[m + 1];
                                Vector2 p5 = gRLT_Entry14.bestNode.ToIntVec2.ToVector2();
                                GRLT_Entry gRLT_Entry15 = dictionary3[m];
                                Vector2 p6 = gRLT_Entry15.bestNode.ToIntVec2.ToVector2();
                                GRLT_Entry gRLT_Entry16 = dictionary3[m + 1];
                                Vector2 p7 = gRLT_Entry16.bestNode.ToIntVec2.ToVector2();
                                Vector2 vector3 = GenGeo.InverseQuadBilinear(p, p4, p2, p5, p3);
                                if (vector3.x >= -0.0001f && vector3.x <= 1.0001f && vector3.y >= -0.0001f && vector3.y <= 1.0001f)
                                {
                                    vector2 = new Vector2((0f - vector3.x) * (float)num, (vector3.y + (float)m) * 4f);
                                    num10 = m;
                                    break;
                                }
                                Vector2 vector4 = GenGeo.InverseQuadBilinear(p, p4, p6, p5, p7);
                                if (vector4.x >= -0.0001f && vector4.x <= 1.0001f && vector4.y >= -0.0001f && vector4.y <= 1.0001f)
                                {
                                    vector2 = new Vector2(vector4.x * (float)num, (vector4.y + (float)m) * 4f);
                                    num10 = m;
                                    break;
                                }
                            }
                        }
                        if (num10 == int.MinValue)
                        {
                            Log.ErrorOnce("Failed to find all necessary river flow data", 5273133);
                        }
                        array[num9] = vector2.x;
                        array[num9 + 1] = vector2.y;
                    }
                    num9 += 2;
                }
            }
            float[] array2 = new float[cellRect.Area * 2];
            float[] array3 = new float[9]
            {
            0.123317f,
            0.123317f,
            0.123317f,
            0.123317f,
            0.077847f,
            0.077847f,
            0.077847f,
            0.077847f,
            0.195346f
            };
            int num11 = 0;
            for (int n = cellRect.minZ; n <= cellRect.maxZ; n++)
            {
                for (int num12 = cellRect.minX; num12 <= cellRect.maxX; num12++)
                {
                    IntVec3 a2 = new IntVec3(num12, 0, n);
                    float num13 = 0f;
                    float num14 = 0f;
                    float num15 = 0f;
                    for (int num16 = 0; num16 < GenAdj.AdjacentCellsAndInside.Length; num16++)
                    {
                        IntVec3 c = a2 + GenAdj.AdjacentCellsAndInside[num16];
                        if (cellRect.Contains(c))
                        {
                            int num17 = num11 + (GenAdj.AdjacentCellsAndInside[num16].x + GenAdj.AdjacentCellsAndInside[num16].z * cellRect.Width) * 2;
                            if (array[num17] != 0f || array[num17 + 1] != 0f)
                            {
                                num13 += array[num17] * array3[num16];
                                num14 += array[num17 + 1] * array3[num16];
                                num15 += array3[num16];
                            }
                        }
                    }
                    if (num15 > 0f)
                    {
                        array2[num11] = num13 / num15;
                        array2[num11 + 1] = num14 / num15;
                    }
                    num11 += 2;
                }
            }
            array = array2;
            for (int num18 = 0; num18 < array.Length; num18 += 2)
            {
                if (array[num18] != 0f || array[num18 + 1] != 0f)
                {
                    Vector2 vector5 = Rand.InsideUnitCircle * 0.4f;
                    array[num18] += vector5.x;
                    array[num18 + 1] += vector5.y;
                }
            }
            byte[] array4 = new byte[array.Length * 4];
            Buffer.BlockCopy(array, 0, array4, 0, array.Length * 4);
            map.waterInfo.riverOffsetMap = array4;
            map.waterInfo.GenerateRiverFlowMap();
        }

        #endregion
    }
}
