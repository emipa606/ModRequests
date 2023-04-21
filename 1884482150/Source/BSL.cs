using System.Reflection;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;

namespace BSL
{
    [StaticConstructorOnStartup]
    public class BSL
    {
        static BSL()
        {            
            var harmony = new Harmony("com.github.toywalrus.bsl");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }

    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("Resting", MethodType.Getter)]
    public static class Patch
    { 
        [HarmonyPostfix]
        public static void IsResting(Plant __instance, ref bool __result)
        {
            bool isInBasin = false;
            foreach (var cri in __instance.OccupiedRect())
            {
                List<Thing> thingList = __instance.Map.thingGrid.ThingsListAt(cri);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Building_PlantGrower pg = thingList[i] as Building_PlantGrower;
                    if (pg != null)
                    {
                        isInBasin = true;
                        break;
                    }
                }
            }
            if (isInBasin) __result = false;
        }
    }
}