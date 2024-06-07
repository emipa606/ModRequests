using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace zed_0xff.GeneCollectorQOL {
    class GeneCache {
        private static int cacheTick = 0;
        private static HashSet<GeneDef> singleGenes = new HashSet<GeneDef>();
        private static HashSet<GeneDef> combinedGenes = new HashSet<GeneDef>();

        public static float sat => 0.75f;
        public static Color yellow => new Color(sat, sat, 0);
        public static Color green  => new Color(0,   sat, 0);
        public static Color red    => new Color(sat, 0,   0);

        public static bool HasSingleGene(GeneDef gene){
            Update();
            return singleGenes.Contains(gene);
        }

        public static bool HasGene(GeneDef gene){
            Update();
            return singleGenes.Contains(gene) || combinedGenes.Contains(gene);
        }

        public static Color GeneColor(GeneDef gene){
            Update();
            if( singleGenes.Contains(gene) )
                return green;
            if( combinedGenes.Contains(gene) )
                return yellow;
            // if ignored return gray
            return red;
        }

        public static int GenepackStatus(Genepack gp){
            var genes = gp.GeneSet.GenesListForReading;
            if( genes.Count == 1 ){
                if( !HasSingleGene(genes[0]) ){
                    return 1;
                } else {
                    return 0;
                }
            } else {
                if( genes.All((GeneDef g) => GeneCache.HasGene(g))){
                    // all genes already known
                    return 0;
                }
                return 2;
            }
        }

        public static void Update(){
            int tick = GenTicks.TicksGame;
            if ( cacheTick == tick )
                return;

            cacheTick = tick;
            singleGenes.Clear();

            foreach( Map map in Find.Maps.Where(m => m.IsPlayerHome) ){
                //  Get genebanks
                List<Thing> banks = new List<Thing>(map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.GenepackHolder)));
                if ((banks == null) || (banks.Count == 0))
                    continue;

                //  Loop through buildings
                foreach (Thing b in banks)
                {
                    //  Get the genepacks
                    CompGenepackContainer comp = b.TryGetComp<CompGenepackContainer>();
                    foreach (Genepack gp in comp.ContainedGenepacks)
                    {
                        if( gp.GeneSet.GenesListForReading.Count == 1){
                            singleGenes.Add(gp.GeneSet.GenesListForReading[0]);
                        } else {
                            foreach (GeneDef g in gp.GeneSet.GenesListForReading) {
                                combinedGenes.Add(g);
                            }
                        }
                    }
                }
            }
        }

    }
}
