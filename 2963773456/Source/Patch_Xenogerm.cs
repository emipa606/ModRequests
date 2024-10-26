using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;

namespace zed_0xff.GeneCourier {
    [HarmonyPatch(typeof(Xenogerm), nameof(Xenogerm.Initialize))]
    static class Patch_Initialize
    {
        static readonly GeneDef geneCourier = DefDatabase<GeneDef>.GetNamed("GeneCourier");

        static void Postfix(ref Xenogerm __instance, List<Genepack> genepacks, string xenotypeName, XenotypeIconDef iconDef)
        {
            if( genepacks == null )
                return;

            if( !(genepacks.Any((Genepack gp) => gp.GeneSet.GenesListForReading.Contains(geneCourier))) )
                return;

            List<GeneSet> genesets = new List<GeneSet>();
            foreach(Genepack gp in genepacks){
                List<GeneDef> genes = gp.GeneSet.GenesListForReading;
                if( genes.Count == 0 )
                    continue;

                genesets.Add(gp.GeneSet.Copy());
            }
            GeneCache.Add(__instance, genesets);
        }
    }

    [HarmonyPatch(typeof(Xenogerm), nameof(Xenogerm.ExposeData))]
    static class Patch_ExposeData
    {
        static void Postfix(ref Xenogerm __instance){
            GeneCache.ExposeData(__instance);
        }
    }
}
