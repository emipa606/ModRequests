using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Text;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_StockholmSyndrome : HediffCompProperties
    {
        public float severityIncreasePerDay;

        public bool showHoursUntilStockholm;

        public HediffCompProperties_StockholmSyndrome()
        {
            compClass = typeof(HediffComp_StockholmSyndrome);
        }
    }

    public class HediffComp_StockholmSyndrome : HediffComp
    {
        protected const int SeverityUpdateInterval = 200;

        private HediffCompProperties_StockholmSyndrome Props => (HediffCompProperties_StockholmSyndrome)props;

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (Props.showHoursUntilStockholm)
                {
                    return Mathf.RoundToInt(HoursUntilStockholm()).ToString() + "h";
                }
                return null;
            }
        }

      

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (base.Pawn.IsHashIntervalTick(SeverityUpdateInterval))
            {
                float num = SeverityIncreasePerDay();
                num *= 0.00333333341f; // Converts daily increase to per-tick
                severityAdjustment += num;
            }

            // Remove the hediff when severity reaches 100%
            if (parent.Severity >= 1f)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }

        protected virtual float SeverityIncreasePerDay()
        {
            return Props.severityIncreasePerDay;
        }

        private float HoursUntilStockholm()
        {
            if (parent.Severity >= 1f)
                return 0f; // Already at 100% severity

            return (1f - parent.Severity) / Mathf.Abs(SeverityIncreasePerDay()) * 24f;
        }

        public override string CompDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompDebugString());
            if (!base.Pawn.Dead)
            {
                stringBuilder.AppendLine("Stockholm Syndrome severity/day: " + SeverityIncreasePerDay().ToString("F3"));
                stringBuilder.AppendLine("Hours until full Stockholm: " + HoursUntilStockholm().ToString("F1"));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}