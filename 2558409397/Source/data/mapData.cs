using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using RimWorld.Planet;

namespace YayoNature
{
    public class mapData : IExposable
    {
        private Map m;

        // Data
        List<string> s_ar_b = null;
        List<BiomeDef> _ar_b = null;
        public List<BiomeDef> ar_b
        {
            get
            {
                if (_ar_b == null)
                {
                    if (s_ar_b != null)
                    {
                        _ar_b = (from s in s_ar_b where BiomeDef.Named(s) != null select BiomeDef.Named(s)).ToList();
                    }
                    else
                    {
                        _ar_b = makeBiomeList();
                        s_ar_b = (from b in _ar_b select b.defName).ToList();
                    }
                }
                return _ar_b;
            }
            set
            {
                _ar_b = value;
                s_ar_b = _ar_b == null ? null : (from b in _ar_b select b.defName).ToList();
            }
        }

        public bool change = false;
        public string s_prev_biome = "";
        public BiomeDef _prev_biome;
        public BiomeDef prev_biome
        {
            get
            {
                if(_prev_biome == null)
                {
                    _prev_biome = BiomeDef.Named(s_prev_biome);
                }
                return _prev_biome;
            }
            set
            {
                _prev_biome = value;
                s_prev_biome = value.defName;
            }
        }
        public float prev_temp = 0f;
        public float target_temp = 0f;
        public int tick = 0;
        public int rpStartIndex = -1;
        public int tickLength = 0;
        public int nextStartChangeTick = 1;
        public Dictionary<IntVec3, bool> dic_c_gen = new Dictionary<IntVec3, bool>();
        public Dictionary<IntVec3, ThingDef> dic_c_thing = new Dictionary<IntVec3, ThingDef>();




        // Data Save
        public void ExposeData()
        {
            Scribe_Collections.Look(ref s_ar_b, $"s_ar_b", LookMode.Value);
            Scribe_Values.Look<bool>(ref change, $"change", false, false);
            Scribe_Values.Look<string>(ref s_prev_biome, $"s_prev_biome", "", false);
            Scribe_Values.Look<float>(ref prev_temp, $"prev_temp", 0f, false);
            Scribe_Values.Look<float>(ref target_temp, $"target_temp", 0f, false);
            Scribe_Values.Look<int>(ref tick, $"tick", 0, false);
            Scribe_Values.Look<int>(ref rpStartIndex, $"rpStartIndex", -1, false);
            Scribe_Values.Look<int>(ref tickLength, $"tickLength", 0, false);
            Scribe_Values.Look<int>(ref nextStartChangeTick, $"nextStartChangeTick", 1, false);
            Scribe_Collections.Look(ref dic_c_gen, $"ar_c_gen", LookMode.Value);
            Scribe_Collections.Look<float>(ref ar_elevation, $"ar_elevation", LookMode.Value);
            Scribe_Collections.Look<float>(ref ar_fertility, $"ar_fertility", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ar_elevation == null) ar_elevation = new List<float>();
                if (ar_fertility == null) ar_fertility = new List<float>();
            }
        }

        
        int i;
        int j;
        IntVec3 c = new IntVec3(0, 0, 0);
        bool mapInited = false;

        public void setParent(Map _m)
        {
            m = _m;
            if (rpStartIndex < 0) rpStartIndex = GenRadial.ar_NumCellsInRadius_rcos[m.Size.x];
        }



        
        public void debugChangeImmediately(BiomeDef b)
        {
            startChange();
            m.TileInfo.biome = b;
            while (change)
            {
                tick++;
                changeTile();
            }
            
        }


        public void startChange(BiomeDef _b = null)
        {
            change = true;
            mapInited = false;
            prev_biome = m.TileInfo.biome;
            if(_b == null)
            {
                m.TileInfo.biome = getNextBiome();
            }
            else
            {
                m.TileInfo.biome = _b;
            }
            
            prev_temp = m.TileInfo.temperature;
            target_temp = core.getBiomeTemp(m.TileInfo.biome);
            tick = 0;
            tickLength = GenRadial.RadialPattern_r_length - rpStartIndex;
            reset_dic_c_gen();

            
            //BeachMaker.Cleanup();
            check_nextStartChangeTick();
            resetElevationFertility();
            //prev_biome.baseWeatherCommonalities[0].weather.
            


            if (core.val_notice)
            {
                Find.LetterStack.ReceiveLetter(
                    "biomeChangeStart_t".Translate(),
                    string.Format("biomeChangeStart_d".Translate(),
                    m.TileInfo.feature != null ? m.TileInfo.feature.name : m.Index.ToString(),
                    m.TileInfo.biome.label.Colorize(Color.magenta),
                    $"- {m.TileInfo.biome.label} -\n\n{m.TileInfo.biome.description}"),
                    LetterDefOf.NeutralEvent
                );
            }
        }




        public void resetElevationFertility()
        {
            Log.Message("resetElevationFertility");
            MapGenerator.mapBeingGenerated = m;
            Settlement settlement = Find.WorldObjects.Settlements.Find(a => a.Map == m);
            MapGeneratorDef mapGenerator = settlement.MapGeneratorDef;
            List<GenStepWithParams> ar_genStepWithParams = mapGenerator.genSteps.Select((GenStepDef x) => new GenStepWithParams(x, default(GenStepParams))).ToList();
            ar_genStepWithParams.Concat(settlement.ExtraGenStepDefs);
            GenStepWithParams gs_elevationFertility = ar_genStepWithParams.Find(a => a.def.genStep is GenStep_ElevationFertility);
            if (gs_elevationFertility.def != null) gs_elevationFertility.def.genStep.Generate(m, gs_elevationFertility.parms);
            fertility = AccessTools.PropertyGetter(typeof(MapGenerator), "Fertility").Invoke(null, new object[] { }) as MapGenFloatGrid;
            elevation = AccessTools.PropertyGetter(typeof(MapGenerator), "Elevation").Invoke(null, new object[] { }) as MapGenFloatGrid;
        }



        public void check_nextStartChangeTick()
        {
            if(nextStartChangeTick < core.tickGame)
            {
                nextStartChangeTick += (int)Mathf.Max(1, core.val_changeCycle + Rand.RangeSeeded(-core.val_changeCycleRandom, core.val_changeCycleRandom + 1, Find.World.ConstantRandSeed + core.tickGame)) * GenDate.TicksPerDay;
            }
        }
        public void stopChange()
        {
            change = false;
            m.TileInfo.temperature = target_temp;
            m.weatherDecider.StartInitialWeather();
            /*
            i = 50000;
            for (j = 0; j < i; j++)
            {
                m.wildPlantSpawner.WildPlantSpawnerTick();
            }
            */
            if (core.val_notice) noticeNextBiome();
        }

        public void noticeNextBiome()
        {
            Find.LetterStack.ReceiveLetter(
                    "biomeComingSoon_t".Translate(),
                    string.Format("biomeComingSoon_d".Translate(),
                    m.TileInfo.feature.name,
                    getNextBiome().label.Colorize(Color.magenta),
                    $"{Mathf.Max(1, core.val_changeCycle - core.val_changeCycleRandom)}~{core.val_changeCycle + core.val_changeCycleRandom}",
                    $"- {getNextBiome().label} -\n\n{getNextBiome().description}"),
                    LetterDefOf.NeutralEvent
                );
        }




        public void doTick()
        {
            //if (core.val_testMode && tick % 100 == 0) Log.Message($"change {change}, local tick {tick}, tick {core.tickGame} / {nextStartChangeTick}");
            if (!change) return;
            tick++;
            if (!mapInited)
            {
                Log.Message($"map inited");
                mapInited = true;
                BeachMaker.Init(m);
                RockNoises.Init(m);

                float num = 0.7f;
                ar_roof = new List<RoofThreshold>();
                RoofThreshold roofThreshold = new RoofThreshold();
                roofThreshold.roofDef = RoofDefOf.RoofRockThick;
                roofThreshold.minGridVal = num * 1.14f;
                ar_roof.Add(roofThreshold);
                RoofThreshold roofThreshold2 = new RoofThreshold();
                roofThreshold2.roofDef = RoofDefOf.RoofRockThin;
                roofThreshold2.minGridVal = num * 1.04f;
                ar_roof.Add(roofThreshold2);


                genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
                genStep_ScatterLumpsMineable.maxValue = maxMineableValue;
                float num3 = 10f;
                switch (Find.WorldGrid[m.Tile].hilliness)
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
                genStep_ScatterLumpsMineable.minSpacing = 5f;
                genStep_ScatterLumpsMineable.warnOnFail = false;
                //int num4 = genStep_ScatterLumpsMineable.CalculateFinalCount(m);
                int? num4 = AccessTools.Method(typeof(GenStep_Scatterer), "CalculateFinalCount").Invoke(genStep_ScatterLumpsMineable as GenStep_Scatterer, new object[] { m }) as int?;
                if (num4 != null)
                {
                    for (int i = 0; i < num4; i++)
                    {
                        //if (!genStep_ScatterLumpsMineable.TryFindScatterCell(m, out var result))
                        if (!TryFindScatterCell(m, out var result))
                        {
                            break;
                        }
                        ScatterAt(result, m);
                    }
                }
                
            }

            changeTile();
            
        }

        // 다른 모더의 harmony postfix를 위한 메소드
        public TerrainDef getCustomTerrainDef(Map m, IntVec3 c, Building edifice, MapGenFloatGrid elevation, MapGenFloatGrid fertility, MapGenFloatGrid caves)
        {
            return null;
        }
        TerrainDef td;
        object o;
        GenStep_Terrain genStep_terrain = new GenStep_Terrain();
        public TerrainDef getTerrainDef(Map m, IntVec3 c, Building edifice, MapGenFloatGrid elevation, MapGenFloatGrid fertility, MapGenFloatGrid caves)
        {
            //td = getCustomTerrainDef(m, c, edifice, elevation, fertility, caves);
            //if (td != null) return td;
            return (((edifice == null || edifice.def.Fillage != FillCategory.Full) && !(caves[c] > 0f)) ? TerrainFrom(c, m, elevation[c], fertility[c], null, preferSolid: false) : TerrainFrom(c, m, elevation[c], fertility[c], null, preferSolid: true));
            //return TerrainFrom(c, m, elevation[c], fertility[c], null, preferSolid: false);
        }



        TerrainDef c_td;
        TerrainDef c_utd;
        RiverMaker riverMaker;
        List<RoofThreshold> ar_roof = new List<RoofThreshold>();
        MapGenFloatGrid caves;
        Building edifice;
        TerrainDef terrainDef;
        Plant plant;
        GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable;
        bool c_water => (c_td.IsWater || (c_utd != null && c_utd.IsWater));
        Building building;

        void changeTile()
        {
            m.TileInfo.temperature = (target_temp - prev_temp) * (float)tick / (float)tickLength + prev_temp;

            i = rpStartIndex + tick;

            if (i >= GenRadial.RadialPattern_r_length)
            {
                stopChange();
                return;
            }
            
            c = m.Center + GenRadial.RadialPattern_r[i];
            //if (core.val_testMode && tick % 100 == 0) Log.Message($"change tile {c}");

            if (c.x < 0 || c.x >= m.Size.x || c.z < 0 || c.z >= m.Size.z) return;

            set_dic_c_gen(c, true);

            c_td = m.terrainGrid.TerrainAt(c);
            c_utd = m.terrainGrid.UnderTerrainAt(c);

            // 강물 변경금지
            if ((c_td.affordances != null && c_td.affordances.Contains(TerrainAffordanceDefOf.MovingFluid)) || (c_utd != null && c_utd.affordances != null && c_utd.affordances.Contains(TerrainAffordanceDefOf.MovingFluid))) return;

            // 물타일 변경금지
            if (!core.val_changeWater && c_water) return;


            MapGenerator.mapBeingGenerated = m;
            //Settlement settlement = Find.WorldObjects.Settlements.Find(a => a.Map == m);
            //IEnumerable<GenStepWithParams> enumerable = settlement.MapGeneratorDef.genSteps.Select((GenStepDef x) => new GenStepWithParams(x, default(GenStepParams)));
            //genStep_terrain = GenStepDefOf.PreciousLump.genStep as GenStep_Terrain;

            //RiverMaker riverMaker = GenerateRiver(m);
            riverMaker = null;
            caves = MapGenerator.Caves;


            edifice = c.GetEdifice(m);
            terrainDef = null;




            if (core.val_changeMt)
            {
                // 산지붕 변경
                if (c.GetRoof(m) != null && c.GetRoof(m).isNatural && c.GetRoof(m).isThickRoof) m.roofGrid.SetRoof(c, RoofDefOf.RoofRockThin);

                if (edifice == null)
                {
                    // 산 생성
                    if (dic_c_thing.ContainsKey(c))
                    {
                        GenSpawn.Spawn(dic_c_thing[c], c, m, WipeMode.FullRefund);
                    }
                    else
                    {
                        tryGenerateRock(c);
                    }

                }
                else
                {
                    // 산 제거
                    if (edifice.def.mineable)
                    {
                        edifice.Destroy();
                    }
                }
            }
            
            


            m.snowGrid.SetDepth(c, 0f); // 눈 제거

            // 타일 결정
            terrainDef = getTerrainDef(m, c, edifice, elevation, fertility, caves);
            if (!core.val_changeWater && terrainDef.IsWater)
            {
                terrainDef = TerrainThreshold.TerrainAtValue(m.Biome.terrainsByFertility, fertility[c]);
                if (core.val_testMode) Log.Message($"target terrain is water -> {terrainDef.label}");
            }

            // 건설된 바닥 유지
            // c 셀이 두겹이라면 덮혀 있다면 SetUnderTerrain
            if (c_utd != null)
            {
                m.terrainGrid.SetUnderTerrain(c, terrainDef);
            }
            else
            {
                SetTerrain(c, terrainDef, m, m.terrainGrid.topGrid, Traverse.Create(m.terrainGrid).Field("underGrid").GetValue<TerrainDef[]>());
            }



            // 식물 제거
            plant = c.GetPlant(m);
            if (plant != null && (c.Impassable(m) || (!core.ar_doNotDestroy_thingDefs.Contains(plant.def) && !plant.IsCrop))) plant.Destroy();






            // 식물 생성
            float progress = (float)(i - rpStartIndex) / (GenRadial.RadialPattern_r_length - rpStartIndex);
            if (progress > 0.85f && !Rand.ChanceSeeded(0.001f, core.tickGame))
            {
                float currentPlantDensity = m.wildPlantSpawner.CurrentPlantDensity;
                float currentWholeMapNumDesiredPlants = m.wildPlantSpawner.CurrentWholeMapNumDesiredPlants;


                //i = Mathf.Min(5000, (int)(50f / (float)core.val_changeTick / (float)currentPlantDensity));
                //Log.Message($"{i}");

                i = Mathf.RoundToInt(150 * progress);
                for (j = 0; j < i; j++)
                {
                    m.wildPlantSpawner.WildPlantSpawnerTick();
                }

                //m.wildPlantSpawner.CheckSpawnWildPlantAt(c, currentPlantDensity, currentWholeMapNumDesiredPlants, setRandomGrowth: true);

                /*
                List<Twelfth> ar_Twelfth = GenTemperature.TwelfthsInAverageTemperatureRange(m.Tile, 6f, 42f);

                // float f_chance = currentPlantDensity * currentWholeMapNumDesiredPlants / (float)m.Area;
                float f_chance = (float)ar_Twelfth.Count / 12f;
                
                Log.Message($"{f_chance} = currentPlantDensity {currentPlantDensity}, currentWholeMapNumDesiredPlants/m.Area {currentWholeMapNumDesiredPlants / (float)m.Area}");
                if (Rand.ChanceSeeded(f_chance, core.tickGame))
                {
                    m.wildPlantSpawner.CheckSpawnWildPlantAt(c, currentPlantDensity, currentWholeMapNumDesiredPlants, setRandomGrowth: true);
                    //newPlantCount++;
                }
                */

            }

            MapGenerator.mapBeingGenerated = null;
            //m.regionAndRoomUpdater.Enabled = false;
        }





        private TerrainDef TerrainFrom(IntVec3 c, Map m, float elevation, float fertility, RiverMaker river, bool preferSolid)
        {
            TerrainDef terrainDef = null;
            if (river != null)
            {
                terrainDef = river.TerrainAt(c, recordForValidation: true);
            }
            /*
            if (terrainDef == null && preferSolid)
            {
                return RockDefAt(c).building.naturalTerrain;
            }
            */
            TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, m.Biome);
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
            for (int i = 0; i < m.Biome.terrainPatchMakers.Count; i++)
            {
                terrainDef2 = m.Biome.terrainPatchMakers[i].TerrainAt(c, m, fertility);
                if (terrainDef2 != null)
                {
                    return terrainDef2;
                }
            }
            if (elevation > 0.55f && elevation < 0.61f)
            {
                return TerrainDefOf.Gravel;
            }
            if (core.val_changeMt && elevation >= 0.61f)
            {
                
                return RockDefAt(c).building.naturalTerrain;
            }
            terrainDef2 = TerrainThreshold.TerrainAtValue(m.Biome.terrainsByFertility, fertility);
            if (terrainDef2 != null)
            {
                return terrainDef2;
            }
            return TerrainDefOf.Sand;
        }



        public void SetTerrain(IntVec3 c, TerrainDef newTerr, Map map, TerrainDef[] topGrid, TerrainDef[] underGrid)
        {
            if (core.using_advBiome)
            {
                try
                {
                    ((Action)(() =>
                    {
                        // do
                        var terrainDef = map.terrainGrid.TerrainAt(c);
                        if (terrainDef is ActiveTerrain.SpecialTerrain)
                        {
                            map.GetComponent<ActiveTerrain.SpecialTerrainList>().Notify_RemovedTerrainAt(c);
                        }
                        
                    }))();
                }
                catch (TypeLoadException) { }
            }


            if (newTerr == null)
            {
                Log.Error(string.Concat("Tried to set terrain at ", c, " to null."));
                return;
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor)?.Delete();
            }
            int num = map.cellIndices.CellToIndex(c);

            if (newTerr.layerable)
            {
                if (underGrid[num] == null)
                {
                    if (topGrid[num].passability != Traversability.Impassable)
                    {
                        underGrid[num] = topGrid[num];
                    }
                    else
                    {
                        underGrid[num] = TerrainDefOf.Sand;
                    }
                }
            }
            else
            {
                underGrid[num] = null;
            }
            topGrid[num] = newTerr;
            //AccessTools.Method(typeof(TerrainGrid), "DoTerrainChangedEffects").Invoke(map.terrainGrid, new object[] { c });
            DoTerrainChangedEffects(c, map);



            if (core.using_advBiome)
            {
                try
                {
                    ((Action)(() =>
                    {
                        // do
                        ActiveTerrain.SpecialTerrain special;
                        special = newTerr as ActiveTerrain.SpecialTerrain;
                        if (special != null)
                        {
                            map.GetComponent<ActiveTerrain.SpecialTerrainList>().RegisterAt(special, c);
                        }
                        
                    }))();
                }
                catch (TypeLoadException) { }
            }
        }







        private void DoTerrainChangedEffects(IntVec3 c, Map map)
        {
            map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Terrain, regenAdjacentCells: true, regenAdjacentSections: false);
            List<Thing> thingList = c.GetThingList(map);
            for (int num = thingList.Count - 1; num >= 0; num--)
            {
                /*
                if (thingList[num].def.category == ThingCategory.Plant && map.fertilityGrid.FertilityAt(c) < thingList[num].def.plant.fertilityMin && !ar_doNotDestroy_thingDefs.Contains(thingList[num].def))
                {
                    thingList[num].Destroy();
                } else if
                */
                if (thingList[num].def.category == ThingCategory.Filth && !FilthMaker.TerrainAcceptsFilth(TerrainAt(c, map), thingList[num].def))
                {
                    thingList[num].Destroy();
                }
                else if ((thingList[num].def.IsBlueprint || thingList[num].def.IsFrame) && !GenConstruct.CanBuildOnTerrain(thingList[num].def.entityDefToBuild, thingList[num].Position, map, thingList[num].Rotation, null, ((IConstructible)thingList[num]).EntityToBuildStuff()))
                {
                    thingList[num].Destroy(DestroyMode.Cancel);
                }
            }
            map.pathing.RecalculatePerceivedPathCostAt(c);
            CellBoolDrawer drawerInt = Traverse.Create(map.terrainGrid).Field("drawerInt").GetValue<CellBoolDrawer>();
            if (drawerInt != null)
            {
                drawerInt.SetDirty();
            }
            map.fertilityGrid.Drawer.SetDirty();
            Region regionAt_NoRebuild_InvalidAllowed = map.regionGrid.GetRegionAt_NoRebuild_InvalidAllowed(c);
            if (regionAt_NoRebuild_InvalidAllowed != null && regionAt_NoRebuild_InvalidAllowed.Room != null)
            {
                regionAt_NoRebuild_InvalidAllowed.Room.Notify_TerrainChanged();
            }
        }
        public TerrainDef TerrainAt(IntVec3 c, Map m)
        {
            return m.terrainGrid.topGrid[m.cellIndices.CellToIndex(c)];
        }
















        int k;
        BiomeDef getNextBiome()
        {
            i = ar_b.IndexOf(m.TileInfo.biome);
            for (k = 0; k < ar_b.Count; k++)
            {
                i++;
                if (i >= ar_b.Count) i -= ar_b.Count;
                if (!core.ar_b_no.Contains(ar_b[i]))
                {
                    break;
                }
                
            }

            return ar_b[i];
        }
        List<BiomeDef> makeBiomeList()
        {
            BiomeDef b = m.Biome;
            List<BiomeDef> ar_tmp = new List<BiomeDef>();
            ar_tmp.AddRange(core.ar_b);
            List<BiomeDef> ar_tmp2 = new List<BiomeDef>();

            ar_tmp.Remove(b);

            while (ar_tmp.Count > 0)
            {
                i = Rand.RangeSeeded(0, ar_tmp.Count, Find.World.ConstantRandSeed + core.tickGame);
                ar_tmp2.Add(ar_tmp[i]);
                ar_tmp.RemoveAt(i);
            }
            ar_tmp2.Add(b);


            // test
            //ar_tmp2[0] = BiomeDefOf.Desert;
            //ar_tmp2[1] = BiomeDefOf.SeaIce;

            Log.Message($"-----------map biome-------------");
            foreach (BiomeDef b2 in ar_tmp2)
            {
                Log.Message($"({core.ar_b.IndexOf(b2)}){b2.label} def : {b2.defName}");
            }
            Log.Message($"------------------------");

            return ar_tmp2;
        }



















        public bool get_dic_c_gen(IntVec3 c)
        {
            try
            {
                return dic_c_gen[c];
            }
            catch
            {
                remake_dic_c_gen();
                return dic_c_gen[c];
            }
        }
        public void set_dic_c_gen(IntVec3 c, bool b)
        {
            try
            {
                dic_c_gen[c] = b;
            }
            catch
            {
                remake_dic_c_gen();
                dic_c_gen[c] = b;
            }
        }
        void remake_dic_c_gen()
        {
            dic_c_gen = new Dictionary<IntVec3, bool>();
            for (int i = 0; i < m.Size.x; i++)
            {
                for (int j = 0; j < m.Size.z; j++)
                {
                    dic_c_gen.Add(new IntVec3(i, 0, j), false);
                }
            }
        }
        void reset_dic_c_gen()
        {
            try
            {
                foreach (IntVec3 c in dic_c_gen.Keys.ToList())
                {
                    dic_c_gen[c] = false;
                }
            }
            catch
            {
                remake_dic_c_gen();
            }
        }










        List<float> ar_elevation = new List<float>();
        MapGenFloatGrid _elevation;
        MapGenFloatGrid elevation
        {
            get
            {
                if (_elevation == null)
                {
                    _elevation = new MapGenFloatGrid(m);
                    if (m != null)
                    {
                        if(ar_elevation.Count >= m.cellIndices.NumGridCells)
                        {
                            Log.Message($"load elevation by array");
                            try { for (int i = 0; i < m.cellIndices.NumGridCells; i++) _elevation[m.cellIndices.IndexToCell(i)] = ar_elevation[i]; }
                            catch { }
                        }
                        else
                        {
                            resetElevationFertility();
                        }
                    }
                }
                return _elevation;
            }
            set
            {
                _elevation = value;
                float[] tmp = new float[m.cellIndices.NumGridCells];
                try { for (int i = 0; i < m.cellIndices.NumGridCells; i++) tmp[i] = value[m.cellIndices.IndexToCell(i)]; }
                catch { }
                ar_elevation = tmp.ToList();

            }
        }
        List<float> ar_fertility = new List<float>();
        MapGenFloatGrid _fertility;
        MapGenFloatGrid fertility
        {
            get
            {
                if (_fertility == null)
                {
                    _fertility = new MapGenFloatGrid(m);
                    if (m != null)
                    {
                        if (ar_fertility.Count >= m.cellIndices.NumGridCells)
                        {
                            Log.Message($"load fertility by array");
                            try { for (int i = 0; i < m.cellIndices.NumGridCells; i++) _fertility[m.cellIndices.IndexToCell(i)] = ar_fertility[i]; }
                            catch { }
                        }
                        else
                        {
                            resetElevationFertility();
                        }
                    }
                }
                return _fertility;
            }
            set
            {
                _fertility = value;
                float[] tmp = new float[m.cellIndices.NumGridCells];
                try { for (int i = 0; i < m.cellIndices.NumGridCells; i++) tmp[i] = value[m.cellIndices.IndexToCell(i)]; }
                catch { }
                ar_fertility = tmp.ToList();
            }
        }

















        private float maxMineableValue = float.MaxValue;
        private class RoofThreshold
        {
            public RoofDef roofDef;

            public float minGridVal;
        }
        public bool tryGenerateRock(IntVec3 c)
        {
            if (m.TileInfo.WaterCovered)
            {
                return false;
            }
            bool spawned = false;
            //m.regionAndRoomUpdater.Enabled = false;
            float num = 0.7f;
            
            //MapGenFloatGrid caves = MapGenerator.Caves;



            float num2 = elevation[c];
            if (!(num2 > num))
            {
                return spawned;
            }
            
            if (caves[c] <= 0f)
            {
                if (dic_c_thing.ContainsKey(c))
                {
                    GenSpawn.Spawn(dic_c_thing[c], c, m, WipeMode.FullRefund);
                }
                else
                {
                    GenSpawn.Spawn(RockDefAt(c), c, m, WipeMode.FullRefund);
                }
                
                spawned = true;
            }

            
            for (int i = 0; i < ar_roof.Count; i++)
            {
                
                if (num2 > ar_roof[i].minGridVal)
                {
                    
                    m.roofGrid.SetRoof(c, ar_roof[i].roofDef);
                    
                    break;
                }
            }
            



            BoolGrid visited = new BoolGrid(m);
            List<IntVec3> toRemove = new List<IntVec3>();
            
            /*
            if (visited[c] || !IsNaturalRoofAt(c, m))
            {
                return spawned;
            }
            toRemove.Clear();
            m.floodFiller.FloodFill(c, (IntVec3 x) => IsNaturalRoofAt(x, m), delegate (IntVec3 x)
            {
                visited[x] = true;
                toRemove.Add(x);
            });
            
            if (toRemove.Count < 20)
            {
                for (int j = 0; j < toRemove.Count; j++)
                {
                    m.roofGrid.SetRoof(toRemove[j], null);
                }
            }
            */

            /*
            GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
            genStep_ScatterLumpsMineable.maxValue = maxMineableValue;
            float num3 = 10f;
            switch (Find.WorldGrid[m.Tile].hilliness)
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
            genStep_ScatterLumpsMineable.Generate(m, parms);
            */
            //m.regionAndRoomUpdater.Enabled = true;
            return spawned;
            
        }

        private bool IsNaturalRoofAt(IntVec3 c, Map map)
        {
            if (c.Roofed(map))
            {
                return c.GetRoof(map).isNatural;
            }
            return false;
        }

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





        protected virtual bool TryFindScatterCell(Map map, out IntVec3 result)
        {
            if (false)
            {

                if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CanScatterAt(x), map, out result))
                {
                    return true;
                }
            }
            else
            {
                if (false)
                {
                    result = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, map, 20, (IntVec3 x) => CanScatterAt(x));
                    return true;
                }
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, (IntVec3 x) => CanScatterAt(x), map, out result))
                {
                    return true;
                }
            }
            return false;


            bool CanScatterAt(IntVec3 c)
            {
                return true;
                bool? b = AccessTools.Method(typeof(GenStep_ScatterLumpsMineable), "CanScatterAt").Invoke(genStep_ScatterLumpsMineable, new object[] { c, m }) as bool?;
                return b == null ? false : b.Value;
            }



        }





        protected void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
        {
            ThingDef thingDef = ChooseThingDef();
            if (thingDef == null)
            {
                return;
            }
            foreach (IntVec3 c2 in GridShapeMaker.IrregularLump(c, map, thingDef.building.mineableScatterLumpSizeRange.RandomInRange))
            {
                if (!dic_c_thing.ContainsKey(c2))
                {
                    dic_c_thing.Add(c2, thingDef);
                }
            }
        }



        protected ThingDef ChooseThingDef()
        {
            
            if (genStep_ScatterLumpsMineable.forcedDefToScatter != null)
            {
                return genStep_ScatterLumpsMineable.forcedDefToScatter;
            }
            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeightWithFallback(delegate (ThingDef d)
            {
                if (d.building == null)
                {
                    return 0f;
                }
                return (d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > float.MaxValue) ? 0f : d.building.mineableScatterCommonality;
            });
        }
























    }
}
