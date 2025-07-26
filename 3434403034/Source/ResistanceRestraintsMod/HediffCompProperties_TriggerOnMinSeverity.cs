using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_TriggerOnMinSeverity : HediffCompProperties
    {
        public HediffDef triggerHediffDef;
        public HediffDef applyHediffDef;
        public float threshold = 0.05f; // Default threshold

        public HediffCompProperties_TriggerOnMinSeverity()
        {
            this.compClass = typeof(HediffComp_TriggerOnMinSeverity);
        }
    }

    public class HediffComp_TriggerOnMinSeverity : HediffComp
    {
        public HediffCompProperties_TriggerOnMinSeverity Props => (HediffCompProperties_TriggerOnMinSeverity)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn == null || Props.triggerHediffDef == null || Props.applyHediffDef == null)
                return;

            Hediff triggerHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.triggerHediffDef);
            if (triggerHediff != null && triggerHediff.Severity < (triggerHediff.def.minSeverity + Props.threshold))
            {
                if (!Pawn.health.hediffSet.HasHediff(Props.applyHediffDef))
                {
                    Pawn.health.AddHediff(Props.applyHediffDef);
                }
            }
        }
    }
}
