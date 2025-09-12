using System;
using RimWorld;
using Verse;

namespace Necro
{
    
    public class IngestionOutcomeDoer_RemoveNecrotion : IngestionOutcomeDoer
    {
        
        public IngestionOutcomeDoer_RemoveNecrotion()
        {
        }

        
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff necronoid_Infection_Hediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.removeThisHediff, false);
            if (necronoid_Infection_Hediff != null)
            {
                pawn.health.hediffSet.hediffs.Remove(necronoid_Infection_Hediff);
            }
        }

        
        public HediffDef removeThisHediff;
    }
}
