using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace Umbrellas {
	[HarmonyPatch(typeof(CompColorable), "Initialize")]
	class DrawColorPatch {
        private static Color RandomColor() {
            return new Color(Random.Range(0f,1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
		static void Postfix (CompColorable __instance) {
            if (__instance.parent.def.Equals(UmbrellaDefOf.Parasol) || __instance.parent.def.Equals(UmbrellaDefOf.FoldableParasol)) {
                __instance.DesiredColor = RandomColor();
            }
        }
	}
    [HarmonyPatch(typeof(ThingWithComps), "DrawColor", MethodType.Getter)]
    class ThingWithCompsDrawColorPatch {
        static void Postfix(ThingWithComps __instance, ref Color __result) {
            if (__instance.def.Equals(UmbrellaDefOf.Parasol) || __instance.def.Equals(UmbrellaDefOf.FoldableParasol)) {
                __result = __instance.GetComp<CompColorable>().Color;
            }
        }
    }
    [HarmonyPatch(typeof(CompColorableUtility), "SetColor")]
    class CompColorableUtilitySetColorPatch {
        private static Color RandomColor() {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        static void Postfix(ref Thing t) {
            if (t.def.Equals(UmbrellaDefOf.Parasol) || t.def.Equals(UmbrellaDefOf.FoldableParasol)) {
                t.TryGetComp<CompColorable>().DesiredColor = RandomColor();
                //note: this will cause the color to be set twice (it's set again in CompColorable.Initialize because otherwise it wouldn't run at all on debug-generated ones).
                //So far this has not caused problems.
            }
        }
    }

    /*[HarmonyPatch(typeof(Thing), "DrawColor", MethodType.Getter)]
    class ThingDefOfDrawColorPatch {
        private static Color RandomColor() {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
        static void Postfix(Thing __instance, ref Color __result) {
            if (__instance.def.Equals(UmbrellaDefOf.Parasol) || __instance.def.Equals(UmbrellaDefOf.FoldableParasol)) {
                __instance.DrawColor = RandomColor(); // not sure why this doesn't change the color more than once but it seems once this is set it erases the get method and just returns the color which is good I guess???
                __result = __instance.DrawColor;
            }
        }
    }*/
}
