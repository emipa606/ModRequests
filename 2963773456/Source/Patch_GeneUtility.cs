using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;

namespace zed_0xff.GeneCourier {

    // transfer info about source genepacks from a Xenogerm to a pawn's gene
    [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.ImplantXenogermItem))]
    static class Patch_ImplantXenogermItem
    {
        static readonly GeneDef geneCourier = DefDatabase<GeneDef>.GetNamed("GeneCourier");

        static void Postfix(Pawn pawn, Xenogerm xenogerm){
            if( pawn == null || pawn.genes == null || xenogerm == null )
                return;

            List<GeneSet> genesets = GeneCache.Get(xenogerm);
            if( genesets == null )
                return;

            foreach( Gene gene in pawn.genes.GenesListForReading ){
                if( gene is Gene_Courier gc) {
                    gc.AddGenesets(genesets);
                    break;
                }
            }
        }
    }
}
