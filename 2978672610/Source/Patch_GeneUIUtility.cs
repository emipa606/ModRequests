using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Reflection;

// draw yellow bg on genes in a combined genepack that we don't have as separate genes
namespace zed_0xff.GeneCollectorQOL {
    [HarmonyPatch(typeof(GeneUIUtility), "DrawGeneDef_NewTemp")]
    static class Patch_Color
    {
        public static float sat => 0.5f;
        public static Color yellow => new Color(sat, sat, 0f, 0.4f);
        public static Color green  => new Color(0f,  sat, 0f, 0.4f);

        static void Prefix(GeneDef gene, Rect geneRect, GeneType geneType, Func<string> extraTooltip, bool doBackground, bool clickable, bool overridden)
        {
            if( ModConfig.Settings.patchGeneAssembler_colorizeCombinedPart && !GeneCache.HasSingleGene(gene) ){
                Widgets.DrawBoxSolid(geneRect, yellow);
            }
        }
    }
}
