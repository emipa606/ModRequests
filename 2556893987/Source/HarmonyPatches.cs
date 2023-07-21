using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace yayoPlanet
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("yayoPlanet");
            harmony.PatchAll(Assembly.GetExecutingAssembly());


        }

    }

    [HarmonyPatch(typeof(StorytellerUtility), "DefaultThreatPointsNow")]
    internal class StorytellerUtility_DefaultThreatPointsNow_patch
    {
        [HarmonyPostfix]
        static void Postfix(ref float __result, IIncidentTarget target)
        {
            Map map = target as Map;
            if(map != null && util.isYayoGC(map))
            {
                __result *= util.threatMultiply;
            }
            


        }
    }





    /*

    [HarmonyPatch(typeof(TileTemperaturesComp), "OutdoorTemperatureAcceptableFor")]
    class TileTemperaturesComp_OutdoorTemperatureAcceptableFor_patch
    {
        static void Postfix(TileTemperaturesComp __instance, int tile, ThingDef animalRace, ref bool __result)
        {
            float temp = __instance.GetOutdoorTemp(tile);

            if (temp < animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
            {
                __result = false;
            }
            else if (temp > animalRace.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
            {
                __result = false;
            }
        }
    }

    */





}
