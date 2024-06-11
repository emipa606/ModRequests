using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;

namespace ARoM_SpaceTimeMage
{
    [StaticConstructorOnStartup]
    internal class HediffComp_SpaceTimeMageHediff : HediffComp
    {
        private List<HediffDef> immuneHediffDefs = new List<HediffDef> { HediffDef.Named("TM_DeathReversalHD") };

        private uint minuteTick = 3600;

        public string labelCap
        {
            get
            {
                return base.Def.LabelCap;
            }
        }

        public string label
        {
            get
            {
                return base.Def.label;
            }
        }


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Find.TickManager.TicksGame % minuteTick == 0)
            {
                foreach (var immuneHediffDef in immuneHediffDefs)
                {
                    if (this.Pawn.health.hediffSet.HasHediff(immuneHediffDef))
                    {
                        this.Pawn.health.hediffSet.GetFirstHediffOfDef(immuneHediffDef).Severity = 0;
                    }
                }
            }
        }

    }
}
