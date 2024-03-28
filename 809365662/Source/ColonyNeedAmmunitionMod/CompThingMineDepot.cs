using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColonyNeedAmmunitionMod
{
    public class CompThingMineDepot : ThingComp
    {
        public CompProperties_ThingMineDepot Props {
            get {
                return (CompProperties_ThingMineDepot)this.props;
            }
        }
    }
}
