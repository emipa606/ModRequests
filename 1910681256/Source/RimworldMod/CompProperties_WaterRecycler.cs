using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
    class CompProperties_WaterRecycler : CompProperties
    {
        public int RecycleTicks;

        public CompProperties_WaterRecycler()
        {
            this.compClass = typeof(CompWaterRecycler);
        }
    }
}
