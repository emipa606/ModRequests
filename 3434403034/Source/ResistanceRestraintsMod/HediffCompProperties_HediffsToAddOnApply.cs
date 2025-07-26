using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_HediffsToAddOnApply : HediffCompProperties
    {
        public List<HediffDef> additionalHediffs = new List<HediffDef>(); // List of hediffs to add

        public HediffCompProperties_HediffsToAddOnApply()
        {
            this.compClass = typeof(HediffComp_HediffsToAddOnApply);
        }
    }


    public class HediffComp_HediffsToAddOnApply : HediffComp
    {
        public HediffCompProperties_HediffsToAddOnApply Props => (HediffCompProperties_HediffsToAddOnApply)this.props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Pawn == null || Props.additionalHediffs == null || Props.additionalHediffs.Count == 0)
                return;

            foreach (HediffDef hediffDef in Props.additionalHediffs)
            {
                if (!Pawn.health.hediffSet.HasHediff(hediffDef))
                {
                    Pawn.health.AddHediff(hediffDef);
                }
            }
        }
    }
}
