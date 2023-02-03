using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Linq;
using RimWorld.Planet;
using System.Collections.Generic;
using HarmonyLib;
using System;


namespace YayoNature
{

    public class core : ModBase
    {
        public override string ModIdentifier => "YayoNature";

        public static bool using_advBiome = false;

        static core()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.PackageId.ToLower().Contains("Mlie.AdvancedBiomes".ToLower())))
            {
                Log.Message("# advanced biomes using : true");
                using_advBiome = true;
            }
            else
            {
                Log.Message("# advanced biomes using : false");
            }
        }


        private SettingHandle<bool> val_worldBiome_s;
        public static bool val_worldBiome;

        private SettingHandle<bool> val_notice_s;
        public static bool val_notice;


        private SettingHandle<bool> val_changeWater_s;
        public static bool val_changeWater;

        private SettingHandle<bool> val_changeMt_s;
        public static bool val_changeMt;



        private SettingHandle<int> val_changeCycle_s;
        public static int val_changeCycle;

        private SettingHandle<int> val_changeCycleRandom_s;
        public static int val_changeCycleRandom;


        private SettingHandle<int> val_changeTick_s;
        public static int val_changeTick;


        private SettingHandle<bool> val_detectMods_s;
        public static bool val_detectMods;

        private SettingHandle<bool> val_testMode_s;
        public static bool val_testMode;


        static public List<BiomeDef> ar_b;
        static public List<float> ar_b_temp;

        static public List<ThingDef> ar_doNotDestroy_thingDefs = new List<ThingDef>();



        static public Dictionary<BiomeDef, SettingHandle<bool>> dic_biomeSetting = new Dictionary<BiomeDef, SettingHandle<bool>>();
        static public List<BiomeDef> ar_b_no = new List<BiomeDef>();


        public override void DefsLoaded()
        {
            val_worldBiome_s = Settings.GetHandle<bool>("val_worldBiome", "val_worldBiome_t".Translate(), "val_worldBiome_d".Translate(), true);
            val_notice_s = Settings.GetHandle<bool>("val_notice", "val_notice_t".Translate(), "val_notice_d".Translate(), true);

            val_changeWater_s = Settings.GetHandle<bool>("val_changeWater", "val_changeWater_t".Translate(), "val_changeWater_d".Translate(), true);
            val_changeMt_s = Settings.GetHandle<bool>("val_changeMt", "val_changeMt_t".Translate(), "val_changeMt_d".Translate(), false);

            val_changeCycle_s = Settings.GetHandle<int>("val_changeCycle", "val_changeCycle_t".Translate(), "val_changeCycle_d".Translate(), 60);
            val_changeCycleRandom_s = Settings.GetHandle<int>("val_changeCycleRandom", "val_changeCycleRandom_t".Translate(), "val_changeCycleRandom_d".Translate(), 15);
            val_changeTick_s = Settings.GetHandle<int>("val_changeTick", "val_changeTick_t".Translate(), "val_changeTick_d".Translate(), 8);

            val_detectMods_s = Settings.GetHandle<bool>("val_detectMods", "val_detectMods_t".Translate(), "val_detectMods_d".Translate(), true);
            val_testMode_s = Settings.GetHandle<bool>("val_testMode", "val_testMode_t".Translate(), "val_testMode_d".Translate(), false);


            dic_biomeSetting = new Dictionary<BiomeDef, SettingHandle<bool>>();
            foreach (BiomeDef b in from b2 in DefDatabase<BiomeDef>.AllDefs where b2.canBuildBase select b2)
            {
                dic_biomeSetting.Add(b, Settings.GetHandle<bool>($"noBiome_{b.defName}", $"  - {b.label}".Translate(), $"{b.description}", true));
            }


            SettingsChanged();

            ar_doNotDestroy_thingDefs.Add(ThingDefOf.Plant_TreeAnima);
            ar_doNotDestroy_thingDefs.Add(ThingDefOf.Plant_GrassAnima);
            ar_doNotDestroy_thingDefs.Add(ThingDefOf.Plant_TreeGauranlen);
            ar_doNotDestroy_thingDefs.Add(ThingDefOf.Plant_PodGauranlen);


        }

        public override void SettingsChanged()
        {
            val_worldBiome = val_worldBiome_s.Value;
            val_notice = val_notice_s.Value;

            val_changeWater = val_changeWater_s.Value;
            val_changeMt = val_changeMt_s.Value;

            val_changeCycle_s.Value = Mathf.Clamp(val_changeCycle_s.Value, 1, 999);
            val_changeCycle = val_changeCycle_s.Value;

            val_changeCycleRandom_s.Value = Mathf.Clamp(val_changeCycleRandom_s.Value, 0, 999);
            val_changeCycleRandom = val_changeCycleRandom_s.Value;

            val_changeTick_s.Value = Mathf.Clamp(val_changeTick_s.Value, 1, 30);
            val_changeTick = val_changeTick_s.Value;


            val_detectMods = val_detectMods_s.Value;

            val_testMode = val_testMode_s.Value;


            ar_b_no = new List<BiomeDef>();
            foreach (KeyValuePair<BiomeDef, SettingHandle<bool>> d in dic_biomeSetting.ToList())
            {
                if (!d.Value)
                {
                    ar_b_no.Add(d.Key);
                    Log.Message($"'{d.Key.label}' will do not appear");
                }
            }


        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();

            worldData wd = dataUtility.GetData(Current.Game.World);
            List<string> new_biomeDefForCheckChange = (from b2 in DefDatabase<BiomeDef>.AllDefs select b2.defName).ToList();

            Log.Message($"# data mod list : {(wd.biomeDefForCheckChange != null ? string.Join(", ", wd.biomeDefForCheckChange) : "")}");
            Log.Message($"# new mod list : {(new_biomeDefForCheckChange != null ? string.Join(", ", new_biomeDefForCheckChange) : "")}");

            if (wd.biomeDefForCheckChange == null || wd.biomeDefForCheckChange.Count <= 0)
            {
                wd.biomeDefForCheckChange = new_biomeDefForCheckChange;
                Log.Message($"# save mod list : {(wd.biomeDefForCheckChange != null ? string.Join(", ", wd.biomeDefForCheckChange) : "")}");
            }
            else if(val_detectMods && wd.biomeDefForCheckChange.Count != new_biomeDefForCheckChange.Count)
            {
                bool bl = false;
                foreach (string s in new_biomeDefForCheckChange)
                {
                    if (!wd.biomeDefForCheckChange.Contains(s))
                    {
                        bl = true;
                    }
                }
                foreach (string s in wd.biomeDefForCheckChange)
                {
                    if (!new_biomeDefForCheckChange.Contains(s))
                    {
                        bl = true;
                    }
                }

                if (bl)
                {
                    Log.Message($"# biome mod list is changed. reset planet for new biome list");
                    resetPlanet();
                    wd.biomeDefForCheckChange = new_biomeDefForCheckChange;
                    Log.Message($"# save mod list : {(wd.biomeDefForCheckChange != null ? string.Join(", ", wd.biomeDefForCheckChange) : "")}");
                }
                
            }

            ar_b = wd.ar_b;
            ar_b_temp = wd.ar_b_temp;


            // 모든 베이스 랜덤 바이옴
            trySetRandomBiomeForAllBase();

            // 버그 방지를 위한 초기화
            MapGenFloatGrid mapGenFloatGrid = null;
            MapGenerator.SetVar("Elevation", mapGenFloatGrid);
            MapGenerator.SetVar("Fertility", mapGenFloatGrid);
            MapGenerator.SetVar("Caves", mapGenFloatGrid);
        }

        static public void trySetRandomBiomeForAllBase(bool forced = false)
        {
            // 모든 베이스 랜덤 바이옴
            worldData wd = dataUtility.GetData(Current.Game.World);
            if (val_worldBiome && (forced || !wd.worldRandomSetuped))
            {
                Log.Message($"# world Random biome Setuped");
                wd.worldRandomSetuped = true;
                foreach (Settlement s in Find.World.worldObjects.Settlements)
                {
                    if (s.Faction == Find.FactionManager.OfPlayer) continue;
                    BiomeDef b = core.getRandomBiome();
                    Find.WorldGrid[s.Tile].biome = b;
                    Find.WorldGrid[s.Tile].temperature = getBiomeTemp(b);
                }
                foreach (Site s in Find.World.worldObjects.Sites)
                {
                    BiomeDef b = core.getRandomBiome();
                    Find.WorldGrid[s.Tile].biome = b;
                    Find.WorldGrid[s.Tile].temperature = getBiomeTemp(b);
                }
            }
        }


        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);
            
        }

        static public void resetPlanet(bool render = false)
        {
            WorldGenStep_Terrain wg = new WorldGenStep_Terrain();
            //wg.GenerateFresh(Current.Game.World.info.seedString);
            List<Tile> tiles = Find.WorldGrid.tiles;
            
            foreach (Tile t in Find.WorldGrid.tiles)
            {
                i = Find.WorldGrid.tiles.IndexOf(t);
                t.biome = AccessTools.Method(typeof(WorldGenStep_Terrain), "BiomeFrom").Invoke(new WorldGenStep_Terrain(), new object[] { t, i }) as BiomeDef;
            }

            dataUtility.GetData(Current.Game.World).ar_b = null;
            ar_b = dataUtility.GetData(Current.Game.World).ar_b;
            dataUtility.GetData(Current.Game.World).ar_b_temp = null;
            ar_b_temp = dataUtility.GetData(Current.Game.World).ar_b_temp;

            trySetRandomBiomeForAllBase(true);

            if (render)(Traverse.Create(Find.World.renderer).Field("layers").GetValue<List<WorldLayer>>().Find(a => a is WorldLayer_Terrain) as WorldLayer_Terrain).RegenerateNow();
            //Current.Game.World.renderer.RegenerateAllLayersNow();

            

            foreach (Map m in Find.Maps)
            {
                if (m.IsPlayerHome)
                {
                    mapData md = dataUtility.GetData(m);
                    md.ar_b = null;
                }
            }
        }
        

        // -----------------------------------------


        static public int tickGame = 0;
        mapData md;

        public override void Tick(int currentTick)
        {
            tickGame = Find.TickManager.TicksGame;
            //Log.Message($"tickSeed Random : {Rand.RangeSeeded(0.1f, 0.9f, tickGame)}");
            //Log.Message($"worldSeed Random : {Rand.RangeSeeded(0.1f, 0.9f, Find.World.ConstantRandSeed)}");
            

            // 첫 사이클 건너뛰기
            if (!val_testMode && tickGame < 5) return;

            if(val_notice && tickGame == GenDate.TicksPerHour)
            {
                foreach (Map m in Find.Maps)
                {
                    if (m.IsPlayerHome)
                    {
                        dataUtility.GetData(m).noticeNextBiome();
                    }
                }
            }

            // 시작
            foreach (Map m in Find.Maps)
            {
                if (m.IsPlayerHome)
                {
                    md = dataUtility.GetData(m);
                    if (md.nextStartChangeTick == tickGame)
                    {
                        // 변화 시작
                        dataUtility.GetData(m).startChange();
                    }
                    else if(md.nextStartChangeTick < tickGame)
                    {
                        // 시작 날짜 초기화
                        md.check_nextStartChangeTick();
                    }

                    if (tickGame % val_changeTick == 0)
                    {
                        // 맵 틱
                        dataUtility.dic_map[m].doTick();
                    }
                }
            }
            
        }

        static public float getBiomeTemp(BiomeDef b)
        {
            return ar_b_temp[ar_b.IndexOf(b)];
        }
        static public void setTileTemp(Tile tile)
        {
            tile.temperature = getBiomeTemp(tile.biome);

        }





        





        static public BiomeDef getRandomBiome()
        {
            List<BiomeDef> ar = new List<BiomeDef>();
            ar.AddRange(core.ar_b);
            ar.RemoveAll(a => core.ar_b_no.Contains(a));
            return ar.RandomElement();
        }


        static int i = 0;
        static int j = 0;

        private struct GRLT_Entry
        {
            public float bestDistance;

            public IntVec3 bestNode;
        }
        




        

    }










}