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
    public static class StatPatches
    {
        [HarmonyPatch(typeof(StatExtension))]
        [HarmonyPatch("GetStatValue", MethodType.Normal)]
        public static class StatExtension_GetStatValue
        {
            [HarmonyPostfix]
            public static void Postfix(Thing thing, StatDef stat, ref float __result)
            {
                // We'll just get an error if not in the play state
                if (Current.ProgramState != ProgramState.Playing)
                    return;

                if (stat.defName != "MarketValue" && stat.defName != "MarketValueIgnoreHp")
                    return;

                if (thing as Trinket != null && thing.Stuff != null && thing.def.costStuffCount > 0)
                {
                    float stuffValue = thing.Stuff.BaseMarketValue;
                    float valuePerStuff = (float)__result / thing.def.costStuffCount;
                    float newValue = stuffValue * valuePerStuff * thing.def.costStuffCount;

                    //Log.Message(String.Format("Trinket: {0}, value: {1}, value per stuff: {2}, stuff value: {3}, new total value: {4}", thing, __result, valuePerStuff, stuffValue, newValue));

                    __result = newValue;
                }
            }
        }
    }
}
