using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace IHaveMaterials
{
    [HarmonyPatch]
    static class Designator_Build_ProcessInput_Patch
    {
        private static readonly MethodInfo CheckCanInteract = AccessTools.Method(typeof(Designator_Build), "CheckCanInteract");
        private static readonly FieldInfo stuffDef = AccessTools.Field(typeof(Designator_Build), "stuffDef");
        private static readonly FieldInfo writeStuff = AccessTools.Field(typeof(Designator_Build), "writeStuff");

        [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.ProcessInput), new Type[] { typeof(Event) })]
        [HarmonyPrefix]
        static bool Prefix(Designator_Build __instance, BuildableDef ___entDef, Event ev)
        {
            if (!(bool)CheckCanInteract.Invoke(__instance, new object[0]))
            {
                return true;
            }

            var thingDef = ___entDef as ThingDef;
            if (thingDef == null || !thingDef.MadeFromStuff)
            {
                return true;
            }

            var vanilla = __instance.Map.resourceCounter.AllCountedAmounts.Keys.Where(d => d.IsStuff && d.stuffProps.CanMake(thingDef) && (DebugSettings.godMode || __instance.Map.listerThings.ThingsOfDef(d).Count > 0));

            // Non-vanilla stuff begins
            var forced = IHaveMaterials.Instance.Stuff.Where(d => d.Value && d.Key.stuffProps.CanMake(thingDef)).Select(d => d.Key);
            if (IHaveMaterials.Instance.StonyFromMap)
            {
                var rockWalls = Find.World.NaturalRockTypesIn(__instance.Map.Tile);
                var rockChunks = rockWalls.Select(t => t.building.mineableThing);
                var rockBlocks = rockChunks.SelectMany(d => d.butcherProducts.Select(p => p.thingDef));
                forced = forced.Union(rockChunks.Concat(rockBlocks).Where(d => d.IsStuff && d.stuffProps.CanMake(thingDef)));
            }

            var merged = vanilla.Union(forced).OrderByDescending(d => d.stuffProps.commonality).ThenBy(d => d.BaseMarketValue);
            // Non-vanilla stuff ends

            var options = merged.Select(item =>
            {
                string label;

                if (__instance.sourcePrecept == null)
                    label = GenLabel.ThingLabel(___entDef, item);
                else 
                    label = "ThingMadeOfStuffLabel".Translate(item.LabelAsStuff, __instance.sourcePrecept.Label);

                return new FloatMenuOption(label.CapitalizeFirst(), delegate
                {
                    BaseProcessInput(__instance, ev);
                    Find.DesignatorManager.Select(__instance);
                    stuffDef.SetValue(__instance, item);
                    writeStuff.SetValue(__instance, true);
                }, item)
                {
                    tutorTag = "SelectStuff-" + thingDef.defName + "-" + item.defName
                };
            }).ToList();

            if (options.Count == 0)
            {
                return true;
            }

            FloatMenu floatMenu = new FloatMenu(options);
            floatMenu.vanishIfMouseDistant = true;
            floatMenu.onCloseCallback = delegate
            {
                writeStuff.SetValue(__instance, true);
            };
            Find.WindowStack.Add(floatMenu);
            Find.DesignatorManager.Select(__instance);

            return false;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Designator), nameof(Designator.ProcessInput), new Type[] { typeof(Event) })]
        static void BaseProcessInput(Designator instance, Event ev)
        {
            throw new NotImplementedException("reverse patch somehow failed");
        }
    }
}
