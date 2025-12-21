using DubsBadHygiene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
    class CompWaterRecycler : ThingComp
    {
        public override void CompTick()
        {
            base.CompTick();
            if(this.parent.IsHashIntervalTick(((CompProperties_WaterRecycler)this.props).RecycleTicks))
            {
                CompWaterStorage storage = this.parent.TryGetComp<CompWaterStorage>();
                if(storage != null && storage.CapPercent < 1)
                    storage.WaterStorage++;
            }
        }
    }
}
