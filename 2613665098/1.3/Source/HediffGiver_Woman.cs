using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HediffApplier
{
    public class HediffGiver_Woman : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (PawnChecker.MeetReq(pawn))
            {
                if (PawnChecker.HasHediff(pawn))
                {
                    HealthUtility.AdjustSeverity(pawn, this.hediff, 0.1f);
                }
                else
                {
                    Hediff newHediff = HediffMaker.MakeHediff(this.hediff, pawn, null);
                    newHediff.Severity = 0.1f;
                    pawn.health.AddHediff(newHediff, null, null);
                }
            }           
        }
    }
}
