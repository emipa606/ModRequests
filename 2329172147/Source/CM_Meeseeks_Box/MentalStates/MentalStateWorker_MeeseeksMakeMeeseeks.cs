using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class MentalStateWorker_MeeseeksMakeMeeseeks : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
                return false;

            if (!pawn.SpawnedOrAnyParentSpawned)
                return false;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return false;

            CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

            if (memory == null)
                return false;

            Map map = pawn.MapHeld;

            List<Thing> meeseeksBoxes = map.listerThings.AllThings.Where(thing => (thing.def == MeeseeksDefOf.CM_Meeseeks_Box_Thing_Meeseeks_Box)).ToList();

            return (meeseeksBoxes.Count > 0);
        }
    }
}