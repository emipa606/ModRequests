using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_SeverityFromHediff : HediffCompProperties
    {
        public HediffDef affectingHediff; // The hediff that modifies severity
        public float severityIncreasePerDay = 0f; // How much severity increases when the affecting hediff is present
        public float severityDecreasePerDay = 0f; // How much severity decreases when the affecting hediff is absent

        public HediffCompProperties_SeverityFromHediff()
        {
            compClass = typeof(HediffComp_SeverityFromHediff);
        }
    }

    public class HediffComp_SeverityFromHediff : HediffComp
    {
        protected const int SeverityUpdateInterval = 200;

        private HediffCompProperties_SeverityFromHediff Props => (HediffCompProperties_SeverityFromHediff)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (base.Pawn.IsHashIntervalTick(SeverityUpdateInterval))
            {
                float change = SeverityChangePerDay();
                change *= 0.00333333341f; // Convert to per-tick adjustment
                severityAdjustment += change;
            }
        }

        protected virtual float SeverityChangePerDay()
        {
            if (Props.affectingHediff == null) return 0f;

            bool hasAffectingHediff = Pawn.health.hediffSet.HasHediff(Props.affectingHediff);
            return hasAffectingHediff ? Props.severityIncreasePerDay : -Props.severityDecreasePerDay;
        }

        public override string CompDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompDebugString());
            stringBuilder.AppendLine($"Affecting Hediff: {Props.affectingHediff?.defName ?? "None"}");
            stringBuilder.AppendLine($"Severity Change Per Day: {SeverityChangePerDay():F3}");
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
