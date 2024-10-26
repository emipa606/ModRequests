using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace zed_0xff.GeneCourier {
    public class GeneCache : WorldComponent {
        private static Dictionary<int, List<GeneSet>> cache = new Dictionary<int, List<GeneSet>>();
        private static List<Pawn> geneClearQueue = new List<Pawn>();

        public GeneCache(World w) : base(w)
        {
            // clear caches whenever a game is created or loaded.
            if( Prefs.DevMode ) Log.Message("[d] GeneCourier: clearing cache");
            cache.Clear();
            geneClearQueue.Clear();
        }

        public static void EnqueueGeneClear(Pawn p){
            geneClearQueue.Add(p);
        }

        public static void ClearGenes(){
            if( geneClearQueue.Any() ){
                // remove all pawn genes
                foreach( Pawn pawn in geneClearQueue ){
                    if( pawn.genes != null ){
                        pawn.genes.ClearXenogenes();
                    }
                }
                geneClearQueue.Clear();
            }
        }

        public static void Add(Xenogerm xenogerm, List<GeneSet> genesets){
            //Log.Warning("[d] adding " + xenogerm + " with " + genesets.Count + " genesets");
            cache[xenogerm.thingIDNumber] = genesets;
        }

        public static List<GeneSet> Get(Xenogerm xenogerm){
            List<GeneSet> l = null;
            if( cache.TryGetValue(xenogerm.thingIDNumber, out l) && l.Any() ){
                return l;
            }
            return null;
        }

        public static void ExposeData(Xenogerm xenogerm){
            List<GeneSet> l = null;
            if( Scribe.mode == LoadSaveMode.Saving ){
                //Log.Warning("[d] saving " + xenogerm + " destroyed: " + xenogerm.Destroyed);
                if( !xenogerm.Destroyed && cache.TryGetValue(xenogerm.thingIDNumber, out l) && l.Any() ){
                    Scribe_Collections.Look(ref l, "zed_0xff.GeneCourier.genesets");
                }
            } else if (Scribe.mode == LoadSaveMode.LoadingVars ){
                //Log.Warning("[d] loading " + xenogerm);
                Scribe_Collections.Look(ref l, "zed_0xff.GeneCourier.genesets");
                if( l != null && l.Any() ){
                    Add(xenogerm, l);
                }
            }
        }
    }
}
