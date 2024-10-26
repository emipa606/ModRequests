using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace zed_0xff.GeneCourier {
    public class Gene_Courier : Gene {
        public static HediffDef courierHediff = HediffDef.Named("GeneCourier");

        private List<GeneSet> genesets = null;

        public void AddGenesets(List<GeneSet> genesets){
            this.genesets = genesets;
        }

        public override void PostAdd()
        {
            base.PostAdd();
            AddHediffs();
        }

        // there's no TickRare() in genes :(
        public override void Tick()
        {
            base.Tick();

            if (GenTicks.TicksGame % 1000 == 0 ){
                if (base.pawn.Map != null && base.Active && !pawn.Dead){
                    if( pawn.health.hediffSet.GetFirstHediffOfDef(courierHediff) == null ){
                        // pawn has the Courier gene but no Courier hediff
                        // f.ex. when spawning a space refugee
                        AddHediffs(1); // add with shorter min interval
                    } else if( pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogerminationComa) == null ){
                        SpawnItems(base.pawn);
                    }
                }
            }
        }

        private void SpawnItems(Pawn pawn)
        {
            List<Genepack> genepacksToSpawn = new List<Genepack>();
            if( genesets != null && genesets.Any() ){
                // mimic source genepacks used to create this xenogerm
                foreach( GeneSet gs in genesets ){
                    Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                    genepack.Initialize(gs.GenesListForReading);
                    genepacksToSpawn.Add(genepack);
                }
                genesets.Clear();
            } else if (pawn.genes != null) {
                // smth went wrong, spawn each gene in separate genepack
                foreach( Gene g in pawn.genes.Xenogenes ){
                    Genepack genepack = (Genepack)ThingMaker.MakeThing(ThingDefOf.Genepack);
                    genepack.Initialize(new List<GeneDef>{g.def});
                    genepacksToSpawn.Add(genepack);
                }
            }

            // kill the pawn
            if (!pawn.Dead){
                pawn.Kill(null, null);
            }

            Map map = pawn.Corpse.Map;
            var pos = pawn.Corpse.Position;

            for (int i = 0; i < 100; i++) {
                IntVec3 c;
                CellFinder.TryFindRandomReachableCellNear(pos, map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Blood);
            }
            SoundDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(pos, map, false));

            foreach( Genepack gp in genepacksToSpawn ){
                GenPlace.TryPlaceThing(gp, pos, map, ThingPlaceMode.Near);
            }

            // can't remove pawn genes here because next genes still need to be Tick()'ed too, see Pawn_GeneTracker::GeneTrackerTick()
            GeneCache.EnqueueGeneClear(pawn);
        }

        private void AddHediffs(int daysMin = 2*7)
        {
            const int day = 60000;
            int overrideDurationTicks = new IntRange(daysMin*day, 4*7*day).RandomInRange;

            Hediff coma = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogerminationComa);
            if( coma == null ){
                coma = HediffMaker.MakeHediff(HediffDefOf.XenogerminationComa, pawn);
                pawn.health.AddHediff(coma);
            }
            coma.TryGetComp<HediffComp_Disappears>().ticksToDisappear = overrideDurationTicks;
            pawn.health.AddHediff(courierHediff);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref genesets, "zed_0xff.GeneCourier.genesets");
        }
    }
}
