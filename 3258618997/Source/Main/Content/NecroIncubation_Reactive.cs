using System;
using Verse;

namespace Necro
{
    
    public class NecroIncubation_Reactive : HediffComp
    {
        
        
        private bool Pain
        {
            get
            {
                return base.Pawn.health.InPainShock;
            }
        }

        
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (this.Pain)
            {
                base.Pawn.health.AddHediff(this.NecroIncubation, null, null, null);
            }
        }

        
        public NecroIncubation_Reactive()
        {
        }

        
        private HediffDef NecroIncubation = DefDatabase<HediffDef>.GetNamed("Necronoid_Incubation", true);
    }
}
