using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CryptosleepAdaptationGene
{
    public class GeneExtension : DefModExtension
    {
        public List<HediffDef> preventsHediffs;
    }
    public class CryptosleepAdaptationGeneMod : Mod
    {
        public CryptosleepAdaptationGeneMod(ModContentPack pack) : base(pack)
        {
            new Harmony("CryptosleepAdaptationGeneMod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class Gene_PostAdd_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (__instance.Active)
            {
                var extension = __instance.def.GetModExtension<GeneExtension>();
                if (extension?.preventsHediffs != null)
                {
                    for (var i = __instance.pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                    {
                        var hediff = __instance.pawn.health.hediffSet.hediffs[i];
                        if (extension.preventsHediffs.Contains(hediff.def))
                        {
                            __instance.pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
    {
        typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
    })]
    public static class Pawn_HealthTracker_AddHediff_Patch
    {
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
        {
            if (___pawn.genes != null)
            {
                foreach (var gene in ___pawn.genes.GenesListForReading)
                {
                    if (gene.Active)
                    {
                        var extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension?.preventsHediffs != null)
                        {
                            if (extension.preventsHediffs.Contains(hediff.def))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
