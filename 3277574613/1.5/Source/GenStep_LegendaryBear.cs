using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimRedRedemption.MapGeneration.LegendaryHunts
{
    public class GenStep_LegendaryBear : GenStep_Scatterer
    {
        public override int SeedPart
        {
            get
            {
                return 931842770;
            }
        }

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            return base.CanScatterAt(c, map) && c.Standable(map);
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            Pawn bear = PawnGenerator.GeneratePawn(RDR_DefOf.RDR_LegendBearPawn);
            List<Pawn> pawns = new List<Pawn> { bear };

            GenSpawn.Spawn(bear, loc, map, WipeMode.Vanish);

            // Make the bear permanently manhunting
            bear.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, false, false);


            // Reveal the bear's location on the map
            MapGenerator.rootsToUnfog.Add(loc);

            // Set the area of interest
            MapGenerator.SetVar<CellRect>("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
        }
    }
}