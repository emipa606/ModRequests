using System;
using RimWorld;
using Verse;
namespace TeenagerSkincareExpansion
{
    public class IngestionOutcomeDoer_RemoveHediff : IngestionOutcomeDoer
    {
        public HediffDef hediffDefToRemove;
        public float severityReducedBy = 0.5f;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(this.hediffDefToRemove, false);
            if (firstHediffOfDef != null)
            {
                if (firstHediffOfDef.Severity > severityReducedBy)
                {
                    firstHediffOfDef.Severity -= severityReducedBy;
                }
                else
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
        }
    }
}
