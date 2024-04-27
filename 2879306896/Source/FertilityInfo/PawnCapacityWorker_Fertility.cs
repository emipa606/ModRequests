using RimWorld;
using System.Collections.Generic;
using Verse;

namespace FertilityInfo
{
    public class PawnCapacityWorker_Fertility : PawnCapacityWorker
    {
        public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
        {
            return diffSet.pawn.GetStatValue(StatDefOf.Fertility, true, -1);
        }

        public override bool CanHaveCapacity(BodyDef body)
        {
            return true;
        }
    }
}
