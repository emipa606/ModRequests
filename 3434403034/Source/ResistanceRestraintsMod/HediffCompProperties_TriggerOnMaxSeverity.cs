using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_TriggerOnMaxSeverity : HediffCompProperties
    {
        public HediffDef triggerHediffDef;
        public HediffDef applyHediffDef;
        public bool removeHediffAtMaxSeverity = false; // New optional bool to remove hediff
        public HediffDef removeHediffDef; // Optional specific hediff to remove

        public HediffCompProperties_TriggerOnMaxSeverity()
        {
            this.compClass = typeof(HediffComp_TriggerOnMaxSeverity);
        }
    }

    public class HediffComp_TriggerOnMaxSeverity : HediffComp
    {
        public HediffCompProperties_TriggerOnMaxSeverity Props => (HediffCompProperties_TriggerOnMaxSeverity)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn == null || Props.triggerHediffDef == null)
                return;

            Hediff triggerHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.triggerHediffDef);
            if (triggerHediff != null && triggerHediff.Severity >= triggerHediff.def.maxSeverity)
            {
                // Apply a new hediff if defined
                if (Props.applyHediffDef != null && !Pawn.health.hediffSet.HasHediff(Props.applyHediffDef))
                {
                    Pawn.health.AddHediff(Props.applyHediffDef);
                }

                // Remove a hediff if specified
                if (Props.removeHediffAtMaxSeverity)
                {
                    Hediff hediffToRemove = Props.removeHediffDef != null
                        ? Pawn.health.hediffSet.GetFirstHediffOfDef(Props.removeHediffDef)
                        : triggerHediff;

                    if (hediffToRemove != null)
                    {
                        Pawn.health.RemoveHediff(hediffToRemove);
                    }
                }
            }
        }
    }
}
