using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnaerobicDigester.Harmony.Patches
{
    [HarmonyPatch(typeof(CompRefuelable))]
    [HarmonyPatch("Refuel")]
    [HarmonyPatch(new Type[] {typeof(List<Thing>)})]
    public class PatchRefuel
    {
        [HarmonyPrefix]
        public static void Prefix(List<Thing> fuelThings, CompRefuelable __instance, out Dictionary<Thing, int> __state)
        {
            __state = new Dictionary<Thing, int>();
            if (__instance.props is CompProperties_SuperRefuelable props && props.isSuperRefuelable)
            {
                foreach (Thing t in fuelThings.FindAll(thing => thing.def.HasModExtension<SuperFuelExtension>()))
                {
                    __state.Add(t, t.stackCount);
                    Log.Message($"Added {t.def.label} with stack count {t.stackCount}");
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(List<Thing> fuelThings, CompRefuelable __instance, Dictionary<Thing, int> __state)
        {
            Dictionary<Thing, int> stackDiffs = new Dictionary<Thing, int>();
            Log.Message($"State dict length = {__state.Count}");
            foreach (Thing t in __state.Keys.ToList().FindAll(
                thing => (thing.def.HasModExtension<SuperFuelExtension>())))
            {
                if (t.Spawned)
                {
                    stackDiffs.Add(t, __state[t] - t.stackCount);
                }
                else
                {
                    stackDiffs.Add(t, __state[t]);
                }
            }

            int fuelToAdd = 0;
            foreach (KeyValuePair<Thing, int> item in stackDiffs)
            {
                Thing curThing = item.Key;
                int consumed = item.Value;

                if (!curThing.def.HasModExtension<SuperFuelExtension>())
                {
                    throw new InvalidDataException(
                        $"Thing {curThing.def.label} has no SuperFuelExtension despite having one at the start of the patch");
                }

                fuelToAdd += consumed * curThing.def.GetModExtension<SuperFuelExtension>().additionalFuel;
            }
            
            __instance.Refuel(fuelToAdd);
                
        }
    }
}
