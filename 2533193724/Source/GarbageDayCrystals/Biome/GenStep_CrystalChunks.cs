using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Noise;

namespace Crystosentient.Biome
{
    public class GenStep_CrystalChunks : GenStep_RockChunks
    {

        #region Overrides
        private ModuleBase freqFactorNoise;

        private const float ThreshLooseRock = 0.55f;

        private const float PlaceProbabilityPerCell = 0.006f;

        private const float RubbleProbability = 0.5f;

        public override int SeedPart => 1898758716;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!map.TileInfo.WaterCovered)
            {
                freqFactorNoise = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
                freqFactorNoise = new ScaleBias(1.0, 1.0, freqFactorNoise);
                NoiseDebugUI.StoreNoiseRender(freqFactorNoise, "rock_chunks_freq_factor");
                MapGenFloatGrid elevation = MapGenerator.Elevation;
                foreach (IntVec3 allCell in map.AllCells)
                {
                    float num = 0.006f * freqFactorNoise.GetValue(allCell);
                    if (elevation[allCell] < 0.55f && Rand.Value < num)
                    {
                        GrowLowRockFormationFrom(allCell, map);
                    }
                }
                freqFactorNoise = null;
            }
        }

        private void GrowLowRockFormationFrom(IntVec3 root, Map map)
        {
            ThingDef filth_RubbleRock = ThingDefOf.Filth_RubbleRock;
            ThingDef rockThing = Find.World.NaturalRockTypesIn(map.Tile).RandomElement().building.mineableThing;
            Rot4 random = Rot4.Random;
            MapGenFloatGrid elevation = MapGenerator.Elevation;
            IntVec3 intVec = root;
            

            while (true)
            {
                Rot4 random2 = Rot4.Random;
                if (random2 == random)
                {
                    continue;
                } //If i take this line to 53 out i get straight chunk lines out of rockthing gens
                intVec += random2.FacingCell;
                if (map.Biome.defName == "GDCRYST_BIOME_GemGardenAmethyst" && intVec.InBounds(map) && !map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                {
                 
                    Rot4 ranWall = Rot4.Random;
                    IntVec3 intVecWall = root;
                    IntVec3 intVecWallRan = intVecWall += ranWall.FacingCell + new IntVec3(0, 0, -4);
                    if (intVecWallRan.InBounds(map) && !map.terrainGrid.TerrainAt(intVecWallRan).affordances.Contains(TerrainAffordanceDefOf.Bridgeable) && !map.terrainGrid.TerrainAt(intVecWallRan).affordances.Contains(TerrainAffordanceDefOf.Light))
                    {
                        ThingDef wallThing = DefDatabase<ThingDef>.GetNamed("GDCRYST_BUILDABLE_WallAmethystRough");
                        for (int i = 0; i < new Random().Next(1,1); i++)
                        {
                            GenSpawn.Spawn(wallThing, intVecWallRan, map);
                        }
                    }

                    Rot4 ranFilth = Rot4.Random;
                    IntVec3 intVecFilth = root;
                    IntVec3 intVecFilthRan = intVecFilth += ranFilth.FacingCell;
                    IntVec3 intVecFilthNoStackRan = intVecFilthRan + ranFilth.FacingCell;
                    IntVec3 intVecFilthNoStackRanMore = intVecFilthNoStackRan + ranFilth.FacingCell;
                    if (intVecFilthRan.InBounds(map) && !map.terrainGrid.TerrainAt(intVecFilthRan).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    {
                        for (int i = 0; i < new Random().Next(1, 3); i++)
                        {
                            Thing localFilth = intVecFilthRan.GetFirstThing(map, Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth);
                            if (localFilth == null)                             
                            {
                                FilthMaker.TryMakeFilth(intVecFilthRan, map, Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth);
                            }
                            if (localFilth == null && intVecFilthNoStackRan.InBounds(map))
                            {
                               // Causes giant filth stacks
                               // ThingDef filthThingDef = Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth;
                               // Thing filthThing = ThingMaker.MakeThing(filthThingDef, null);
                               // GenPlace.TryPlaceThing(filthThing, intVecFilthNoStackRan, map, ThingPlaceMode.Near, null, null, default(Rot4));
                                FilthMaker.TryMakeFilth(intVecFilthNoStackRan, map, Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth);
                            }
                            if (localFilth == null && intVecFilthNoStackRanMore.InBounds(map))
                            {
                                FilthMaker.TryMakeFilth(intVecFilthNoStackRanMore, map, Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth);
                            }
                        }
                    }

                }
                if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null || elevation[intVec] > 0.55f || !map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                {
                    break;
                }
                else
                {
                    GenSpawn.Spawn(rockThing, intVec, map);
                }
                IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
                foreach (IntVec3 b in adjacentCellsAndInside)
                {
                    if (!(Rand.Value < 0.5f))
                    {
                        continue;
                    }
                    IntVec3 c = intVec + b;
                    if (!c.InBounds(map))
                    {
                        continue;
                    }
                    bool flag = false;
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing = thingList[j];
                        if (thing.def.category != ThingCategory.Plant && thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Pawn)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {

                        if (map.Biome.defName == "GDCRYST_BIOME_GemGardenAmethyst")
                        {
                            FilthMaker.TryMakeFilth(c, map, Crystosentient.Dictionary.DefOfThing.GDCRYST_FILTH_AmethystGemFilth);
                        }
                        else
                            FilthMaker.TryMakeFilth(c, map, filth_RubbleRock);
                    }
                }
            }
        }
        #endregion
    }
}