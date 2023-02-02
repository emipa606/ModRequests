﻿using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Collections.Generic;
using RimWorld.Planet;
using System.Linq;
using System;
using RimWorld.BaseGen;

// So, let's comment this code, since it uses Harmony and has moderate complexity

namespace MechanoidAssimilation
{

    /*This Harmony Prefix allows us to remove rock filth
 */
    [HarmonyPatch(typeof(GenStep_RockChunks))]
    [HarmonyPatch("GrowLowRockFormationFrom")]
    public static class MechanoidAssimilation_GenStep_RockChunks_GrowLowRockFormationFrom_Patch
    {
        [HarmonyPrefix]
        public static bool RemoveRockFilth(ref IntVec3 root, ref Map map)

        {
            if ((map.Biome.defName == "MA_MechanoidAssimilation"))
            {
                ThingDef mineableThing = Find.World.NaturalRockTypesIn(map.Tile).RandomElement<ThingDef>().building.mineableThing;
                Rot4 random = Rot4.Random;
                MapGenFloatGrid elevation = MapGenerator.Elevation;
                IntVec3 intVec = root;
                for (; ; )
                {
                    Rot4 random2 = Rot4.Random;
                    if (!(random2 == random))
                    {
                        intVec += random2.FacingCell;
                        if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null)
                        {
                            break;
                        }
                        if (elevation[intVec] > 0.55f)
                        {
                            return false;
                        }
                        if (!map.terrainGrid.TerrainAt(intVec).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                        {
                            return false;
                        }
                        GenSpawn.Spawn(mineableThing, intVec, map, WipeMode.Vanish);
                        foreach (IntVec3 b in GenAdj.AdjacentCellsAndInside)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                IntVec3 c = intVec + b;
                                if (c.InBounds(map))
                                {
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
                                        FilthMaker.TryMakeFilth(c, map, DefDatabase<ThingDef>.GetNamed("MA_SlagRubble"), 1);
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
            else return true;

        }

    }

 
}
