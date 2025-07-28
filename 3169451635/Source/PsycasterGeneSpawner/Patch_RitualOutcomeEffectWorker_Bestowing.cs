using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PsycasterGeneSpawner {
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_Bestowing), nameof(RitualOutcomeEffectWorker_Bestowing.Apply))]
    public class Patch_RitualOutcomeEffectWorker_Bestowing {
        public static void Postfix(LordJob_Ritual jobRitual) {
            LordJob_BestowingCeremony lordJob_BestowingCeremony = (LordJob_BestowingCeremony)jobRitual;
            Pawn target = lordJob_BestowingCeremony.target;
        
            if (target.genes.HasGene(GeneDefOf.Gene_Archotechist)) return;

            target.genes.AddGene(GeneDefOf.Gene_Archotechist, true);
        }
    }
}
