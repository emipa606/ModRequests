using HarmonyLib;
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

    

   

  

   
    /*This Harmony Postfix allows us to create new rock types than only appear in certain biomes
    */
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("NaturalRockTypesIn")]
    public static class MechanoidAssimilation_World_NaturalRockTypesIn_Patch
    {

        public static string[] vanillaRockTypes = { "Marble", "Sandstone", "Limestone", "Granite", "Slate" };
        
        [HarmonyPostfix]
        public static void MakeRocksAccordingToBiome(int tile, ref World __instance, ref IEnumerable<ThingDef> __result)

        {
            if (__instance.grid.tiles[tile].biome.defName == "MA_MechanoidAssimilation")
            {
                List<ThingDef> replacedList = new List<ThingDef>();
                ThingDef item = DefDatabase<ThingDef>.GetNamed("MA_AncientMetals");
                replacedList.Add(item);

                __result = replacedList;
            
            
            }
            else {
                Rand.PushState();
                Rand.Seed = tile;
                List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
                                       where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock && 
                                       !d.IsSmoothed &&  
                                       d.defName != "MA_AncientMetals" && d.defName != "BiomesIslands_CoralRock"
                                       select d).ToList<ThingDef>();
                int num = Rand.RangeInclusive(2, 3);
                if (num > list.Count)
                {
                    num = list.Count;
                }
                List<ThingDef> list2 = new List<ThingDef>();
                for (int i = 0; i < num; i++)
                {
                    ThingDef item = list.RandomElement<ThingDef>();
                    list.Remove(item);
                    list2.Add(item);
                }
                Rand.PopState();
                __result = list2;
                

            }


        }
    }


}
