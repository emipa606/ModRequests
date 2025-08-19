using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CQCTakedown
{
    // Verse.HediffGiver_DOOMRipAndTear


    public class HediffGiver_DOOMRipAndTear : HediffGiver
    {
        public void RipAndTear(Pawn pawn)
        {

            IntRange range = new IntRange(0, parts.Count);
            for (int i = 0; i < 5; i++)
            { //i can be < anything
                pawn.health.DamageDefOf.Crush(100, 100); //last two numbers are damage and armor pen, can be whatever
            }//end for
        }//end RipAndTear

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.BleedRateTotal >= 0.1f)
            {
                HealthUtility.AdjustSeverity(pawn, hediff, hediffSet.BleedRateTotal * 0.001f);
            }
            else
            {
                HealthUtility.AdjustSeverity(pawn, hediff, -0.00033333333f);
            }
        }
    }
}