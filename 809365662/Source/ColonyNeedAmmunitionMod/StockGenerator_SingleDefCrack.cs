using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ColonyNeedAmmunitionMod
{
    class StockGenerator_SingleDefCrack : StockGenerator
    {
        public ThingDef thingDef;

        public StockGenerator_SingleDefCrack(ThingDef def,int min,int max) {
            thingDef = def;
            this.countRange = new IntRange(min, max);
            price = PriceType.Expensive;
        }

        public override IEnumerable<Thing> GenerateThings()
        {
            List<Thing> ret = new List<Thing>();
            ret.Add(ThingMaker.MakeThing(thingDef));
            ret[0].stackCount = this.RandomCountOf(thingDef);
            return ret;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef == this.thingDef;
        }
    }
}
