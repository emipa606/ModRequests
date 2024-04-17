using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhereIsMyWeapon {
    public class ThingPair : IExposable {
        public Thing thing;
        public int count;

        public ThingPair() {

        }

        public ThingPair(Thing thing,int count) {
            this.thing = thing;
            this.count = count;
        }

        public void ExposeData() {
            Scribe_References.Look(ref thing, "thing");
            Scribe_Values.Look(ref count, "count");
        }
    }
}
