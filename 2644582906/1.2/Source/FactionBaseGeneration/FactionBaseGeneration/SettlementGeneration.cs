using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FactionBaseGeneration
{
    public static class SettlementGeneration
    {
        public static LocationDef GetLocationDefForSettlement(MapParent mapParent)
        {
            foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
            {
                Log.Message(locationDef.factionBase + " - " + mapParent.Faction.def);
                if (locationDef.factionBase == mapParent.Faction.def)
                {
                    return locationDef;
                }
            }
            return null;
        }


        public static FileInfo GetPresetFor(MapParent mapParent, out LocationDef locationDef)
        {
            locationDef = GetLocationDefForSettlement(mapParent);
            return GetPresetFor(mapParent, locationDef);
        }

        public static FileInfo GetPresetFor(MapParent mapParent, LocationDef locationDef)
        {
            if (locationDef != null)
            {
                string path = "";
                FileInfo file = null;
                if (locationDef.filePreset != null && locationDef.filePreset.Length > 0)
                {
                    path = Path.GetFullPath(locationDef.modContentPack.RootDir + "/" + locationDef.filePreset);
                    file = new FileInfo(path);
                }
                else if (locationDef.folderWithPresets != null && locationDef.folderWithPresets.Length > 0)
                {
                    path = Path.GetFullPath(locationDef.modContentPack.RootDir + "/" + locationDef.folderWithPresets);
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (directoryInfo.Exists)
                    {
                        file = directoryInfo.GetFiles().RandomElement();
                    }
                }

                if (file != null)
                {
                    return file;
                }
            }
            return null;
        }

        public static void InitialiseLocationGeneration(Map map, FileInfo file, LocationDef locationDef)
        {
            if (locationDef != null && file != null)
            {
                var comp = map.GetComponent<MapComponentGeneration>();
                if (comp.path.Length == 0)
                {
                    comp.DoGeneration = true;
                    comp.path = file.FullName;
                    comp.locationDef = locationDef;
                }
            }
        }
        public static bool IsChunk(Thing item)
        {
            if (item?.def?.thingCategories != null)
            {
                foreach (var category in item.def.thingCategories)
                {
                    if (category == ThingCategoryDefOf.Chunks || category == ThingCategoryDefOf.StoneChunks)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static IntVec3 GetCellCenterFor(List<IntVec3> cells)
        {
            var x_Averages = cells.OrderBy(x => x.x);
            var x_average = x_Averages.ElementAt(x_Averages.Count() / 2).x;
            var z_Averages = cells.OrderBy(x => x.z);
            var z_average = z_Averages.ElementAt(z_Averages.Count() / 2).z;
            var middleCell = new IntVec3(x_average, 0, z_average);
            return middleCell;
        }

        public static IntVec3 GetOffsetPosition(IntVec3 cell, IntVec3 offset)
        {
            return cell + offset;
        }
        public static HashSet<IntVec3> DoSettlementGeneration(Map map, string path, LocationDef locationDef, Faction faction, bool disableFog)
        {
            Log.Message("DoSettlementGeneration");
            if (faction == Faction.OfPlayer || faction == null)
            {
                faction = Faction.OfAncients;
            }

            List<Thing> thingsToDestroy = new List<Thing>();
            HashSet<IntVec3> tilesToProcess = new HashSet<IntVec3>();

            int radiusToClear = 0;
            //
            //foreach (var building in map.listerThings.AllThings
            //    .Where(x => x is Building && x.Faction == faction && !(x is Mineable)))
            //{
            //    foreach (var pos in GenRadial.RadialCellsAround(building.Position, radiusToClear, true))
            //    {
            //        if (GenGrid.InBounds(pos, map))
            //        {
            //            tilesToProcess.Add(pos);
            //        }
            //    }
            //    thingsToDestroy.Add(building);
            //}

            List<Pawn> pawns = new List<Pawn>();
            List<Building> buildings = new List<Building>();
            List<Thing> things = new List<Thing>();
            List<Plant> plants = new List<Plant>();
            Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
            Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();
            HashSet<IntVec3> tilesToSpawnPawnsOnThem = new HashSet<IntVec3>();

            Scribe.loader.InitLoading(path);

            Scribe_Collections.Look<Pawn>(ref pawns, "Pawns", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Building>(ref buildings, "Buildings", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Thing>(ref things, "Things", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Plant>(ref plants, "Plants", LookMode.Deep, new object[0]);

            Scribe_Collections.Look<IntVec3, TerrainDef>(ref terrains, "Terrains",
                LookMode.Value, LookMode.Def, ref terrainKeys, ref terrainValues);
            Scribe_Collections.Look<IntVec3, RoofDef>(ref roofs, "Roofs",
                LookMode.Value, LookMode.Def, ref roofsKeys, ref roofsValues);

            Scribe_Collections.Look<IntVec3>(ref tilesToSpawnPawnsOnThem, "tilesToSpawnPawnsOnThem", LookMode.Value);


            Scribe.loader.FinalizeLoading();
            var cells = new List<IntVec3>(tilesToSpawnPawnsOnThem);
            cells.AddRange(buildings.Select(x => x.Position).ToList());
            var centerCell = GetCellCenterFor(cells);
            var offset = map.Center - centerCell;
            Log.Message($"centerCell: {centerCell}, map.Center: {map.Center}, offset: {offset}");
            if (pawns != null && pawns.Count > 0)
            {
                foreach (var pawn in pawns)
                {
                    try
                    {
                        var position = GetOffsetPosition(pawn.Position, offset);
                        if (GenGrid.InBounds(position, map))
                        {
                            GenSpawn.Spawn(pawn, position, map, WipeMode.Vanish);
                            pawn.SetFaction(faction);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + pawn + " - " + ex);
                    }
                }
            }

            if (tilesToSpawnPawnsOnThem != null && tilesToSpawnPawnsOnThem.Count > 0)
            {
                foreach (var tile in tilesToSpawnPawnsOnThem)
                {
                    var position = GetOffsetPosition(tile, offset);
                    try
                    {
                        if (GenGrid.InBounds(position, map))
                        {
                            var things2 = map.thingGrid.ThingsListAt(position);
                            foreach (var thing in things2)
                            {
                                if (thing is Building || (thing is Plant plant && plant.def != ThingDefOf.Plant_Grass) || IsChunk(thing))
                                {
                                    thingsToDestroy.Add(thing);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + position + " - " + ex);
                    }
                }
            }

            if (buildings != null && buildings.Count > 0)
            {
                foreach (var building in buildings)
                {
                    var position = GetOffsetPosition(building.Position, offset);

                    foreach (var pos in GenRadial.RadialCellsAround(position, radiusToClear, true))
                    {
                        if (GenGrid.InBounds(pos, map))
                        {
                            tilesToProcess.Add(pos);
                        }
                    }
                }
                if (tilesToProcess != null && tilesToProcess.Count > 0)
                {
                    foreach (var pos in tilesToProcess)
                    {
                        var things2 = map.thingGrid.ThingsListAt(pos);
                        foreach (var thing in things2)
                        {
                            if (thing is Building || (thing is Plant plant && plant.def != ThingDefOf.Plant_Grass) || IsChunk(thing))
                            {
                                thingsToDestroy.Add(thing);
                            }
                        }
                        var water = pos.GetTerrain(map);
                        if (water.IsWater)
                        {
                            map.terrainGrid.SetTerrain(pos, TerrainDefOf.Soil);
                        }
                        var terrain = pos.GetTerrain(map);
                        if (terrain != null && map.terrainGrid.CanRemoveTopLayerAt(pos))
                        {
                            map.terrainGrid.RemoveTopLayer(pos, false);
                        }
                        var roof = pos.GetRoof(map);
                        if (roof != null && (!map.roofGrid.RoofAt(pos).isNatural || map.roofGrid.RoofAt(pos) == RoofDefOf.RoofRockThin))
                        {
                            map.roofGrid.SetRoof(pos, null);
                        }
                    }
                }

                if (thingsToDestroy != null && thingsToDestroy.Count > 0)
                {
                    for (int i = thingsToDestroy.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (thingsToDestroy[i].Spawned)
                            {
                                thingsToDestroy[i].DeSpawn(DestroyMode.WillReplace);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Cant despawn: " + thingsToDestroy[i] + " - "
                                + thingsToDestroy[i].Position + "error: " + ex);
                        }
                    }
                }

                foreach (var building in buildings)
                {
                    var position = GetOffsetPosition(building.Position, offset);
                    try
                    {
                        if (GenGrid.InBounds(position, map))
                        {
                            GenSpawn.Spawn(building, position, map, building.Rotation, WipeMode.Vanish);
                            if (building.def.CanHaveFaction)
                            {
                                building.SetFaction(faction);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + building + " - " + ex);
                    }
                }
            }

            if (plants != null && plants.Count > 0)
            {
                foreach (var plant in plants)
                {
                    try
                    {
                        var position = GetOffsetPosition(plant.Position, offset);

                        if (map.fertilityGrid.FertilityAt(position) >= plant.def.plant.fertilityMin)
                        {
                            GenSpawn.Spawn(plant, position, map, WipeMode.Vanish);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + plant + " - " + ex);
                    }
                }
            }

            var containers = map.listerThings.AllThings.Where(x => x is Building_Storage).ToList();
            if (things != null && things.Count > 0)
            {
                foreach (var thing in things)
                {
                    try
                    {
                        var position = GetOffsetPosition(thing.Position, offset);
                        GenSpawn.Spawn(thing, position, map, WipeMode.Vanish);
                        if (!(thing is Filth) && !(thing is Building) && !(thing is Plant) && !(thing is Pawn))
                        {
                            TryDistributeTo(thing, map, containers, faction != Faction.OfPlayer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + thing + " - " + ex);
                    }
                }
            }
            if (terrains != null && terrains.Count > 0)
            {
                foreach (var terrain in terrains)
                {
                    try
                    {
                        var position = GetOffsetPosition(terrain.Key, offset);
                        if (GenGrid.InBounds(position, map))
                        {
                            map.terrainGrid.SetTerrain(position, terrain.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + terrain.Key + " - " + ex);
                    }
                }
            }
            if (roofs != null && roofs.Count > 0)
            {
                foreach (var roof in roofs)
                {
                    try
                    {
                        var position = GetOffsetPosition(roof.Key, offset);

                        if (GenGrid.InBounds(position, map))
                        {
                            map.roofGrid.SetRoof(position, roof.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error in map generating, cant spawn " + roof.Key + " - " + ex);
                    }
                }
            }
            if (locationDef != null && (locationDef.percentOfDamagedWalls.HasValue || locationDef.percentOfDestroyedWalls.HasValue) || locationDef.percentOfDamagedFurnitures.HasValue)
            {
                var walls = map.listerThings.AllThings.Where(x => x.def.IsEdifice() && x.def.defName.ToLower().Contains("wall")).ToList();
                if (locationDef.percentOfDestroyedWalls.HasValue)
                {
                    var percent = locationDef.percentOfDestroyedWalls.Value.RandomInRange * 100f;
                    var countToTake = (int)((percent * walls.Count()) / 100f);
                    var wallsToDestroy = walls.InRandomOrder().Take(countToTake).ToList();
                    for (int num = wallsToDestroy.Count - 1; num >= 0; num--)
                    {
                        walls.Remove(wallsToDestroy[num]);
                        wallsToDestroy[num].DeSpawn();
                    }
                    Log.Message($"wall count: {walls.Count()}, percent: {percent}, countTotake: {countToTake}");
                }
                if (locationDef.percentOfDamagedWalls.HasValue)
                {
                    var percent = locationDef.percentOfDamagedWalls.Value.RandomInRange * 100f;
                    var countToTake = (int)((percent * walls.Count()) / 100f);
                    var wallsToDamage = walls.InRandomOrder().Take(countToTake).ToList();
                    for (int num = wallsToDamage.Count - 1; num >= 0; num--)
                    {
                        var damagePercent = Rand.Range(0.3f, 0.6f);
                        var hitpointsToTake = (int)(wallsToDamage[num].MaxHitPoints * damagePercent);
                        wallsToDamage[num].HitPoints = hitpointsToTake;
                    }

                    Log.Message($"wall count: {walls.Count()}, percent: {percent}, countTotake: {countToTake}");
                }
                if (locationDef.percentOfDamagedFurnitures.HasValue)
                {
                    var furnitures = map.listerThings.AllThings.Where(x => !walls.Contains(x) && x.def.IsBuildingArtificial).ToList();
                    var percent = locationDef.percentOfDamagedFurnitures.Value.RandomInRange * 100f;
                    var countToTake = (int)((percent * furnitures.Count()) / 100f);
                    var furnituresToDamage = furnitures.InRandomOrder().Take(countToTake).ToList();
                    for (int num = furnituresToDamage.Count - 1; num >= 0; num--)
                    {
                        var damagePercent = Rand.Range(0.3f, 0.6f);
                        var hitpointsToTake = (int)(furnituresToDamage[num].MaxHitPoints * damagePercent);
                        furnituresToDamage[num].HitPoints = hitpointsToTake;
                    }
                    Log.Message($"wall count: {walls.Count()}, percent: {percent}, countTotake: {countToTake}");
                }

            }
            Pawn checker = map.mapPawns.PawnsInFaction(Faction.OfPlayer).FirstOrDefault();
            if (checker != null)
            {
                foreach (var item in map.listerThings.AllThings)
                {
                    if (item.IsForbidden(checker))
                    {
                        TryDistributeTo(item, map, containers, faction != Faction.OfPlayer);
                    }
                }
            }
            if (faction.def.HasModExtension<SettlementOptionModExtension>())
            {
                var options = faction.def.GetModExtension<SettlementOptionModExtension>();
                if (options.removeVanillaGeneratedPawns)
                {
                    for (int i = map.mapPawns.PawnsInFaction(faction).Count - 1; i >= 0; i--)
                    {
                        map.mapPawns.PawnsInFaction(faction)[i].DeSpawn(DestroyMode.Vanish);
                    }
                }
                if (options.pawnsToGenerate != null && options.pawnsToGenerate.Count > 0 && tilesToSpawnPawnsOnThem != null && tilesToSpawnPawnsOnThem.Count > 0)
                {
                    foreach (var pawn in options.pawnsToGenerate)
                    {
                        foreach (var i in Enumerable.Range(1, (int)pawn.selectionWeight))
                        {
                            var settler = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawn.kind, faction));
                            try
                            {
                                var pos = tilesToSpawnPawnsOnThem.Where(x => map.thingGrid.ThingsListAt(x)
                                .Where(y => y is Building).Count() == 0).RandomElement();
                                GenSpawn.Spawn(settler, pos, map);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Error in map generating, cant spawn " + settler + " - " + ex);
                            }
                        }
                    }
                }
            }

            foreach (var pawn in map.mapPawns.PawnsInFaction(faction))
            {
                var lord = pawn.GetLord();
                if (lord != null)
                {
                    map.lordManager.RemoveLord(lord);
                }
                var lordJob = new LordJob_DefendBase();
                if (tilesToSpawnPawnsOnThem != null && tilesToSpawnPawnsOnThem.Count > 0)
                {
                    lordJob = new LordJob_DefendBase(faction, tilesToSpawnPawnsOnThem.RandomElement());
                }
                else
                {
                    lordJob = new LordJob_DefendBase(faction, pawn.Position);
                }
                LordMaker.MakeNewLord(faction, lordJob, map, null).AddPawn(pawn);
            }

            if (disableFog != true)
            {
                try
                {
                    FloodFillerFog.DebugRefogMap(map);
                }
                catch
                {
                    foreach (var cell in map.AllCells)
                    {
                        if (!tilesToProcess.Contains(cell) && !(cell.GetFirstBuilding(map) is Mineable))
                        {
                            var item = cell.GetFirstItem(map);
                            if (item != null)
                            {
                                var room = item.GetRoom();
                                if (room != null)
                                {
                                    if (room.PsychologicallyOutdoors)
                                    {
                                        FloodFillerFog.FloodUnfog(cell, map);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tilesToSpawnPawnsOnThem.Select(x => GetOffsetPosition(x, offset)).ToHashSet();
        }

        private static void TryDistributeTo(Thing thing, Map map, List<Thing> containers, bool setForbidden)
        {
            Dictionary<Thing, List<IntVec3>> containerPlaces = new Dictionary<Thing, List<IntVec3>>();
            for (int num = containers.Count - 1; num >= 0; num--)
            {
                var c = containers[num];
                foreach (var pos in c.OccupiedRect().Cells)
                {
                    bool canPlace = true;
                    foreach (var t in pos.GetThingList(map))
                    {
                        if (t != c && !(t is Filth))
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (canPlace)
                    {
                        if (containerPlaces.ContainsKey(c))
                        {
                            containerPlaces[c].Add(pos);
                        }
                        else
                        {
                            containerPlaces[c] = new List<IntVec3> { pos };
                        }
                    }
                }
            }

            if (containerPlaces != null && containerPlaces.Any())
            {
                var container = (Building_Storage)GenClosest.ClosestThing_Global(thing.Position, containerPlaces.Keys, 9999f);
                if (container != null && containerPlaces.TryGetValue(container, out var positions))
                {
                    var choosenPos = positions.RandomElement();
                    containerPlaces[container].Remove(choosenPos);
                    thing.Position = choosenPos;
                    if (setForbidden)
                    {
                        thing.SetForbidden(true);
                    }
                    if (!containerPlaces[container].Any())
                    {
                        containerPlaces.Remove(container);
                    }
                }
            }
        }

        public static List<IntVec3> terrainKeys = new List<IntVec3>();
        public static List<TerrainDef> terrainValues = new List<TerrainDef>();
        public static List<IntVec3> roofsKeys = new List<IntVec3>();
        public static List<RoofDef> roofsValues = new List<RoofDef>();
    }
}

