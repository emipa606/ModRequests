using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Verse;

namespace PsycasterGeneSpawner {
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_AnimaTreeLinking), nameof(RitualOutcomeEffectWorker_AnimaTreeLinking.Apply))]
    public class Patch_RitualOutcomeEffectWorker_AnimaTreeLinking {
        public static void Postfix(LordJob_Ritual jobRitual) {
            Pawn pawn = jobRitual.PawnWithRole("organizer");

            if (pawn.genes.HasGene(GeneDefOf.Gene_Wildspeaker)) return;

            pawn.genes.AddGene(GeneDefOf.Gene_Wildspeaker, true);
        }
    }
}
