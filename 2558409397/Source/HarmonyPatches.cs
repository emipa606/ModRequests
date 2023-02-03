using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;

namespace YayoNature
{


    public class HarmonyPatches : Mod
    {

        public HarmonyPatches(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.yayo.nature");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

    }


    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch("ExposeData")]
    public class patch_map_exposeData
    {
        private static void Postfix(Map __instance)
        {
            dataUtility.GetData(__instance).ExposeData();
        }
    }

    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("ExposeData")]
    public class patch_World_exposeData
    {
        private static void Postfix(World __instance)
        {
            dataUtility.GetData(__instance).ExposeData();
        }
    }

    /*
    [HarmonyPatch(typeof(WildPlantSpawner), "WildPlantSpawnerTickInternal")]
    internal class Patch_WildPlantSpawner_WildPlantSpawnerTickInternal
    {
        [HarmonyPrefix]
        static bool Prefix(WildPlantSpawner __instance)
        {
            if (dataUtility.GetData(Traverse.Create(__instance).Field("map").GetValue<Map>()).change) return false;
            return true;
        }

    }
    */
    

    [HarmonyPatch(typeof(WildPlantSpawner), "CheckSpawnWildPlantAt")]
    internal class Patch_WildPlantSpawner_CheckSpawnWildPlantAt
    {
        static Map map;
        static mapData md;

        private static List<ThingDef> tmpPossiblePlants = AccessTools.StaticFieldRefAccess<List<ThingDef>>(AccessTools.Field(typeof(WildPlantSpawner), "tmpPossiblePlants")).Invoke();
        private static List<KeyValuePair<ThingDef, float>> tmpPossiblePlantsWithWeight = AccessTools.StaticFieldRefAccess<List<KeyValuePair<ThingDef, float>>>(AccessTools.Field(typeof(WildPlantSpawner), "tmpPossiblePlantsWithWeight")).Invoke();
        private static Dictionary<ThingDef, float> distanceSqToNearbyClusters = AccessTools.StaticFieldRefAccess<Dictionary<ThingDef, float>>(AccessTools.Field(typeof(WildPlantSpawner), "distanceSqToNearbyClusters")).Invoke();
        private static FloatRange InitialGrowthRandomRange = AccessTools.StaticFieldRefAccess<FloatRange>(AccessTools.Field(typeof(WildPlantSpawner), "InitialGrowthRandomRange")).Invoke();


        [HarmonyPrefix]
        static bool Prefix(WildPlantSpawner __instance, ref bool __result, IntVec3 c, float plantDensity, float wholeMapNumDesiredPlants, bool setRandomGrowth = false)
        {
            map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            md = dataUtility.GetData(map);
            if (md.change)
            {
                if(!md.get_dic_c_gen(c)) return false;
                //plantDensity = 1f;
            }
            else
            {
                return true;
            }

            //


            if (plantDensity <= 0f || c.GetPlant(map) != null || c.GetCover(map) != null || c.GetEdifice(map) != null || map.fertilityGrid.FertilityAt(c) <= 0f || !PlantUtility.SnowAllowsPlanting(c, map))
            {
                __result = false;
                return false;
            }

            bool cavePlants = (bool)AccessTools.Method(typeof(WildPlantSpawner), "GoodRoofForCavePlant").Invoke(__instance, new object[] { c });
            if ((bool)AccessTools.Method(typeof(WildPlantSpawner), "SaturatedAt").Invoke(__instance, new object[] { c, plantDensity, cavePlants, wholeMapNumDesiredPlants }))
            {
                __result = false;
                return false;
            }

            AccessTools.Method(typeof(WildPlantSpawner), "CalculatePlantsWhichCanGrowAt").Invoke(__instance, new object[] { c, tmpPossiblePlants, cavePlants, plantDensity });
            if (!tmpPossiblePlants.Any())
            {
                __result = false;
                return false;
            }

            AccessTools.Method(typeof(WildPlantSpawner), "CalculateDistancesToNearbyClusters").Invoke(__instance, new object[] { c });
            tmpPossiblePlantsWithWeight.Clear();
            for (int i = 0; i < tmpPossiblePlants.Count; i++)
            {
                float value = (float)AccessTools.Method(typeof(WildPlantSpawner), "PlantChoiceWeight").Invoke(__instance, new object[] { tmpPossiblePlants[i], c, distanceSqToNearbyClusters, wholeMapNumDesiredPlants, plantDensity });
                tmpPossiblePlantsWithWeight.Add(new KeyValuePair<ThingDef, float>(tmpPossiblePlants[i], value));
            }
            if (!tmpPossiblePlantsWithWeight.TryRandomElementByWeight((KeyValuePair<ThingDef, float> x) => x.Value, out var result))
            {
                __result = false;
                return false;
            }
            Plant plant = (Plant)ThingMaker.MakeThing(result.Key);
            plant.Growth = Mathf.Clamp01(Rand.RangeSeeded(0.3f, 1f, core.tickGame));
            if (plant.def.plant.LimitedLifespan)
            {
                plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
            }
            GenSpawn.Spawn(plant, c, map);

            __result = true;
            return false;
        }

    }
    



    

    //[HarmonyPatch(typeof(WildPlantSpawner), "CalculateDistancesToNearbyClusters")]
    //internal class Patch_WildPlantSpawner_CalculateDistancesToNearbyClusters
    //{
    //    static Map map;
    //    static mapData md;


    //    private static Dictionary<ThingDef, List<float>> nearbyClusters = AccessTools.StaticFieldRefAccess<Dictionary<ThingDef, List<float>>>(AccessTools.Field(typeof(WildPlantSpawner), "nearbyClusters")).Invoke();
    //    private static List<KeyValuePair<ThingDef, List<float>>> nearbyClustersList = AccessTools.StaticFieldRefAccess<List<KeyValuePair<ThingDef, List<float>>>>(AccessTools.Field(typeof(WildPlantSpawner), "nearbyClustersList")).Invoke();
    //    private static Dictionary<ThingDef, float> distanceSqToNearbyClusters = AccessTools.StaticFieldRefAccess<Dictionary<ThingDef, float>>(AccessTools.Field(typeof(WildPlantSpawner), "distanceSqToNearbyClusters")).Invoke();


    //    [HarmonyPriority(0)]
    //    static bool Prefix(WildPlantSpawner __instance, IntVec3 c)
    //    {
    //        map = Traverse.Create(__instance).Field("map").GetValue<Map>();
    //        md = dataUtility.GetData(map);
    //        if (!md.change) return true;
    //        nearbyClusters.Clear();
    //        nearbyClustersList.Clear();
    //        int num = GenRadial.NumCellsInRadius(map.Biome.MaxWildAndCavePlantsClusterRadius * 2);
    //        for (int i = 0; i < num; i++)
    //        {
    //            IntVec3 intVec = c + GenRadial.RadialPattern[i];
    //            if (!intVec.InBounds(map))
    //            {
    //                continue;
    //            }
    //            if(!md.get_dic_c_gen(c))
    //            {
    //                continue;
    //            }
    //            List<Thing> list = map.thingGrid.ThingsListAtFast(intVec);
    //            for (int j = 0; j < list.Count; j++)
    //            {
    //                Thing thing = list[j];
    //                if (thing.def.category == ThingCategory.Plant && thing.def.plant.GrowsInClusters)
    //                {
    //                    float item = intVec.DistanceToSquared(c);
    //                    if (!nearbyClusters.TryGetValue(thing.def, out var value))
    //                    {
    //                        value = SimplePool<List<float>>.Get();
    //                        nearbyClusters.Add(thing.def, value);
    //                        nearbyClustersList.Add(new KeyValuePair<ThingDef, List<float>>(thing.def, value));
    //                    }
    //                    value.Add(item);
    //                }
    //            }
    //        }
    //        distanceSqToNearbyClusters.Clear();
    //        for (int k = 0; k < nearbyClustersList.Count; k++)
    //        {
    //            List<float> value2 = nearbyClustersList[k].Value;
    //            value2.Sort();
    //            distanceSqToNearbyClusters.Add(nearbyClustersList[k].Key, value2[value2.Count / 2]);
    //            value2.Clear();
    //            SimplePool<List<float>>.Return(value2);
    //        }
    //        return true;
    //    }

    //}



    //[HarmonyPatch(typeof(WildPlantSpawner), "SaturatedAt")]
    //internal class Patch_WildPlantSpawner_SaturatedAt
    //{
    //    static Map map;
    //    static mapData md;
    //    static List<Thing> ar_p;

    //    [HarmonyPriority(0)]
    //    static bool Prefix(WildPlantSpawner __instance, IntVec3 c, float plantDensity, bool cavePlants, float wholeMapNumDesiredPlants)
    //    {
    //        map = Traverse.Create(__instance).Field("map").GetValue<Map>();
    //        md = dataUtility.GetData(map);
    //        int num = GenRadial.NumCellsInRadius(20f);
    //        if (wholeMapNumDesiredPlants * ((float)num / (float)map.Area) <= 4f || !map.Biome.wildPlantsCareAboutLocalFertility)
    //        {
    //            return (float)map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count >= wholeMapNumDesiredPlants;
    //        }
    //        float numDesiredPlantsLocally = 0f;
    //        int numPlants = 0;
    //        RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), 20f), delegate (Region reg)
    //        {
    //            numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensity);
    //            ar_p = reg.ListerThings.ThingsInGroup(ThingRequestGroup.Plant);
    //            if (md.change) ar_p.RemoveAll(a => !md.get_dic_c_gen(a.Position));
    //            numPlants += ar_p.Count;
    //            return false;
    //        });
    //        return (float)numPlants >= numDesiredPlantsLocally;


    //    }

        

        //static public float GetDesiredPlantsCountIn(Region reg, IntVec3 forCell, float plantDensity)
        //{
        //    return Mathf.Min(reg.GetBaseDesiredPlantsCount() * plantDensity * forCell.GetTerrain(map).fertility, reg.CellCount);
        //}






    //}










    // 월드 오브젝트 생성시
    
    [HarmonyPatch(typeof(WorldObjectsHolder))]
    [HarmonyPatch("Add")]
    public class patch_WorldObjectsHolder_Add
    {
        private static void Postfix(WorldObjectsHolder __instance, WorldObject o)
        {
            if(core.val_worldBiome && Current.ProgramState == ProgramState.Playing)
            {
                if(o is Site || o is Settlement)
                {
                    BiomeDef b = core.getRandomBiome();
                    Find.WorldGrid[o.Tile].biome = b;
                    Find.WorldGrid[o.Tile].temperature = core.getBiomeTemp(b);
                }
            }

        }
    }












    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
    public class Patch_DefGenerator_GenerateImpliedDefs_PreResolve
    {
        public static void Prefix()
        {
            //yayoCombat.patchDef1();
        }

    }


}
