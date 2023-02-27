using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace FactionBaseGeneration
{
    public class Dialog_NameBlueprint : Dialog_Rename
    {
        public Dialog_NameBlueprint(string name, bool includePawns)
        {
            this.name = name;
            this.includePawns = includePawns;
        }

        public void GetRocks(Map map, ref List<Thing> rocks, ref List<Thing> processedRocks)
        {
            List<Thing> rocksToProcess = new List<Thing>();
            foreach (var rock in rocks)
            {
                if (!processedRocks.Contains(rock))
                {
                    foreach (var pos in GenAdj.CellsAdjacent8Way(rock))
                    {
                        var things = map.thingGrid.ThingsListAt(pos);
                        if (things != null && things.Count > 0)
                        {
                            foreach (var thing in things)
                            {
                                if (thing is Mineable && !processedRocks.Contains(thing))
                                {
                                    rocksToProcess.Add(thing);
                                }
                            }
                        }
                    }
                    processedRocks.Add(rock);
                }
            }
            if (rocksToProcess.Count > 0)
            {
                GetRocks(map, ref rocksToProcess, ref processedRocks);
            }
        }

        protected override void SetName(string name)
        {
            this.name = name;
            Map map = Find.CurrentMap;
            List<Pawn> pawns = new List<Pawn>();
            List<Building> buildings = new List<Building>();
            List<Thing> things = new List<Thing>();
            List<Plant> plants = new List<Plant>();
            Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
            Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();
            HashSet<IntVec3> tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();

            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing is Gas || thing is Mote || thing is Filth) continue;
                if (thing is Pawn pawn)
                {
                    if (this.includePawns)
                    {
                        if (pawn.def.race.Humanlike || pawn.Faction == Faction.OfPlayer)
                        {
                            Log.Message("0 Adding " + pawn, true);
                            pawns.Add(pawn);
                        }
                    }
                }
                else if (thing is Plant plant)
                {
                    Zone zone = map.zoneManager.ZoneAt(thing.Position);
                    if (zone != null && zone is Zone_Growing)
                    {
                        Log.Message("1 Adding " + plant, true);
                        plants.Add(plant);
                    }
                }
                else if (thing is Building building && building.Faction == Faction.OfPlayer)
                {
                    Log.Message("2 Adding " + thing, true);
                    buildings.Add(building);
                }
                else if (thing.IsInAnyStorage())
                {
                    Log.Message("3 Adding " + thing, true);
                    things.Add(thing);
                }

            }

            List<Thing> rocks = new List<Thing>();
            List<Thing> processedRocks = new List<Thing>();
            foreach (var thing in buildings)
            {
                foreach (var pos in GenAdj.CellsAdjacent8Way(thing))
                {
                    var things2 = map.thingGrid.ThingsListAt(pos);
                    if (things2 != null && things2.Count > 0)
                    {
                        foreach (var thing2 in things2)
                        {
                            if (thing2 is Mineable)
                            {
                                rocks.Add(thing2);
                            }
                        }
                    }
                }
            }

            GetRocks(map, ref rocks, ref processedRocks);

            foreach (var rock in processedRocks)
            {
                things.Add(rock);
            }

            foreach (IntVec3 intVec in map.AllCells)
            {
                var terrain = intVec.GetTerrain(map);
                if (terrain != null && map.terrainGrid.CanRemoveTopLayerAt(intVec))
                {
                    terrains[intVec] = terrain;
                }

                var roof = intVec.GetRoof(map);
                if (roof != null && !map.roofGrid.RoofAt(intVec).isNatural)
                {
                    roofs[intVec] = roof;
                }
            }

            foreach (IntVec3 homeCell in map.areaManager.Home.ActiveCells)
            {
                tilesToSpawnPawnsOnThem.Add(homeCell);
            }

            var curModName = LoadedModManager.RunningMods.Where(x => x.assemblies.loadedAssemblies.Contains(Assembly.GetExecutingAssembly())).FirstOrDefault().Name;
            ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData x) => x != null && x.Name != null && x.Active && x.Name == curModName);

            string path = Path.GetFullPath(modMetaData.RootDir.ToString() + "/Presets/" + this.name + ".xml");

            Scribe.saver.InitSaving(path, "Blueprint");
            if (this.includePawns)
            {
                Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep, new object[0]);
            }
            Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains",
                LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs",
                LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);
            Scribe_Collections.Look<IntVec3>(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);

            Scribe.saver.FinalizeSaving();
        }

        private string name;
        private bool includePawns;
        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }
}

