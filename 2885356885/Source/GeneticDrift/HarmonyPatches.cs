using System;
using System.Collections.Generic;
using System.Linq;
﻿using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace GeneticDrift
{
    class GeneticDriftHelper
    {
        public static void drift(ref GeneChancesList chances, float multiplier,
                                 Action<GeneDef> adder,
                                 IEnumerable<GeneDef> allGenes)
        {
            foreach(float geneChance in chances)
            {
                var finalChance = geneChance * multiplier;
                if(Rand.Chance(finalChance))
                {
                    adder(allGenes.ElementAt(Rand.Range(0, allGenes.Count())));
                }
                else
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(PregnancyUtility), "GetInheritedGenes",
     new[] { typeof(Pawn), typeof(Pawn), typeof(bool) },
     new [] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out})]
    class GeneticDriftPatch
    {
        [HarmonyPostfix]
        public static void Listener(Pawn father, Pawn mother,
                                    ref List<GeneDef> __result)
        {
            // TODO weight by complexity?
            var instabilityMult = 1f;
            if(GeneticDriftMod.settings.Instability)
                foreach(var gene in __result)
                    if(gene.statFactors != null)
                        foreach(var stat in gene.statFactors)
                            if(stat != null)
                                if(stat.stat == StatDefOf.CancerRate)
                                    instabilityMult *= stat.value;

            GeneticDriftHelper.drift(
                ref GeneticDriftMod.settings.genesBaby.geneChances, instabilityMult,
                __result.Add, GeneticDriftMod.settings.genesBaby.getList());
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    class GeneticDriftPawnPatch
    {
        [HarmonyPostfix]
        public static void Adult(Pawn pawn, XenotypeDef xenotype,
                                  PawnGenerationRequest request)
        {
            if(request.AllowedDevelopmentalStages == DevelopmentalStage.Newborn)
                return;
            if(GeneticDriftMod.settings.BaselinersOnly
               && xenotype != XenotypeDefOf.Baseliner && xenotype != null)
                return;
            var instabilityMult = 1f;
            if(GeneticDriftMod.settings.Instability)
                instabilityMult *= pawn.GetStatValue(StatDefOf.CancerRate);

            GeneticDriftHelper.drift(
                ref GeneticDriftMod.settings.endogenesAdult.geneChances,
                instabilityMult, (gene) => pawn.genes.AddGene(gene, false),
                GeneticDriftMod.settings.endogenesAdult.getList());
            GeneticDriftHelper.drift(
                ref GeneticDriftMod.settings.xenogenesAdult.geneChances,
                instabilityMult, (gene) => pawn.genes.AddGene(gene, true),
                GeneticDriftMod.settings.xenogenesAdult.getList());
        }
    }
}
