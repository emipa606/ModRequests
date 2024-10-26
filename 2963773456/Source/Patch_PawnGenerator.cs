using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace zed_0xff.GeneCourier {

    // add small chance of space refugee to be a gene courier
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] {typeof(PawnGenerationRequest)})]
    static class Patch_GeneratePawn
    {
        static readonly GeneDef geneCourier = DefDatabase<GeneDef>.GetNamed("GeneCourier");

        static void Prefix(ref PawnGenerationRequest request){
            if( request.KindDef != PawnKindDefOf.SpaceRefugee )
                return;
            if( request.ForcedXenogenes != null && request.ForcedXenogenes.Any() )
                return;

            if( Rand.Value > ModConfig.Settings.probSpaceRefugee )
                return;

            int nToAdd = new IntRange(5, 10).RandomInRange;
            int nAdded = 1;

            GeneSet gs = new GeneSet();
            gs.AddGene(geneCourier);

            for( int i=0; i<10; i++){
                foreach( GeneDef g in GeneUtility.GenerateGeneSet().GenesListForReading ){
                    if( nAdded >= nToAdd ) break;
                    nAdded++;
                    gs.AddGene(g);
                }
                if( nAdded >= nToAdd) break;
            }

            foreach( GeneDef g in gs.GenesListForReading )
                request.AddForcedGene(g, true);
        }
    }

    // add small chance of pawn in ancient cryptosleep pod to be a gene courier, probably with some another archite gene
    // runs on map generation
    [HarmonyPatch(typeof(ThingSetMaker_MapGen_AncientPodContents), "Generate")]
    static class Patch_Generate
    {
        static readonly GeneDef geneCourier = DefDatabase<GeneDef>.GetNamed("GeneCourier");

        // well, not 100% archite, but 11x more likely than regular one)
        static GeneSet generateArchiteGeneSet(){
            for( int i=0; i<10; i++){
                GeneSet gs = GeneUtility.GenerateGeneSet();
                if( gs.ArchitesTotal != 0 )
                    return gs;
            }
            return GeneUtility.GenerateGeneSet();
        }

        static void Postfix(ref ThingSetMaker_MapGen_AncientPodContents __instance, ThingSetMakerParams parms, ref List<Thing> outThings){
            foreach( Thing t in outThings ){
                if( t is Pawn pawn && pawn.Faction == Faction.OfAncients ){
                    // non-hostile ancient

                    if( Rand.Value > ModConfig.Settings.probAncient )
                        continue;

                    pawn.genes.AddGene(geneCourier, true);
                    foreach( GeneDef g in generateArchiteGeneSet().GenesListForReading ){
                        pawn.genes.AddGene(g, true);
                    }
                }
            }
        }
    }
}
