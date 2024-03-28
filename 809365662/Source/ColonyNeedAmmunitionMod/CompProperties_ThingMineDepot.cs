using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColonyNeedAmmunitionMod
{
    public class CompProperties_ThingMineDepot : CompProperties
    {
        public String ProduceDefName;

        public int ProduceAmount;

        public int MaxAmount;

        public CompProperties_ThingMineDepot()
        {
            this.compClass = typeof(CompThingMineDepot);
        }
    }
}
