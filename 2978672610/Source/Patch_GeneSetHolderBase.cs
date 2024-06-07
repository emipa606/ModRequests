using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Reflection;

// colorize gene inspect string
namespace zed_0xff.GeneCollectorQOL {
    [HarmonyPatch(typeof(GeneSetHolderBase), nameof(GeneSetHolderBase.GetInspectString))]
    static class Patch_GetInspectString
    {
        static void Postfix(ref GeneSetHolderBase __instance, ref string __result){
            if( !ModConfig.Settings.patchGene_colorize) return;

            GeneSet gs = __instance.GeneSet;
            if (gs == null)
                return;

            __result += "\n";
            foreach( GeneDef g in gs.GenesListForReading ){
                string orig = "  - " + g.label.CapitalizeFirst() + "\n";
                Color color = GeneCache.GeneColor(g);
                string colored = "  - " + g.label.CapitalizeFirst().Colorize(color) + "\n";
                __result = __result.Replace(orig, colored);
            }
            // remove last \n
            __result.Remove(__result.Length - 1, 1);
        }
    }
}
