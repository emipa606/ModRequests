using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection.Emit;

namespace AnimalsLikeHay
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("JP_ALH: Loading..");
            var harmony = new Harmony("AnimalsLikeHayPatch");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(ThingListGroupHelper))]
    public static class ThingListGroupHelperPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Includes")]
        public static void IncludesPatch(ref ThingRequestGroup group, ThingDef def)
        {
            if(group == ThingRequestGroup.FoodSourceNotPlantOrTree && def == ThingDefOf.Hay)
            {
                //Log.Message("Hay check");
                group = ThingRequestGroup.FoodSource;
            }
        }
    }

}