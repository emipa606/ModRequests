using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Tradeable_Trinkets
{
    [StaticConstructorOnStartup]
    public static class RecipeWorkerCounter_Patches
    {
        [HarmonyPatch(typeof(RecipeWorkerCounter))]
        [HarmonyPatch("CountValidThings", MethodType.Normal)]
        public static class RecipeWorkerCounter_CountValidThings
        {
            [HarmonyPrefix]
            public static bool Prefix(RecipeWorkerCounter __instance, List<Thing> things, Bill_Production bill, ThingDef def, ref int __result)
            {
                // In the base game, this is only called when "Look Everywhere" is specified, and it doesn't include stack count so lets just completely override it to be correct
                int num = 0;
                for (int i = 0; i < things.Count; i++)
                {
                    if (__instance.CountValidThing(things[i], bill, def))
                    {
                        Thing innerIfMinified = things[i].GetInnerIfMinified();
                        num += innerIfMinified.stackCount;
                    }
                }
                __result = num;

                return false;
            }
        }
    }
}
