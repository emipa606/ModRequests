using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Clockwork
{
    public class HediffGiver_BrazenRot : HediffGiver
    {
        public enum HediffType { clockwork, steamwork };

        HediffType hediffType;
        public HediffDef hediffDeadly;
        public HediffDef cure;
        public HediffDef cureImplant;
        public float severityPerDay;

        public override void OnIntervalPassed(Pawn pawn, Hediff notUsed)
        {
            if (ClockworkAndSteamSettings.brazenRot)
            {
                if (ClockworkAndSteamSettings.brazenRotIsDeadly)
                {
                    ApplyBrazenRot(pawn, hediffDeadly);
                }
                else
                {
                    ApplyBrazenRot(pawn, hediff);
                }
            }
        }

        public void ApplyBrazenRot(Pawn pawn, HediffDef type)
        {
            if ((hediffType == HediffType.clockwork && ClockworkAndSteamSettings.brazenRotClockwork) || ((hediffType == HediffType.steamwork && ClockworkAndSteamSettings.brazenRotSteamwork)))
            {
                if (pawn.health.hediffSet.HasHediff(cure) || pawn.health.hediffSet.HasHediff(cureImplant)) // If we have cure, remove the hediff
                {
                    if (type != null)
                    {
                        try
                        {
                            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(type);
                            pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                        catch { }
                    }
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, type, severityPerDay * 0.00333333341f * ClockworkAndSteamSettings.brazenRotMultiplier); // Converts severity to severity per day
                }
            }
        }

    }


}
