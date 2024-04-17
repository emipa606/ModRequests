using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhereIsMyWeapon {
    public class ThingList : IExposable {
        public List<ThingPair> list = new List<ThingPair>();

        public void Add(Thing t,int count) {
            list.Add(new ThingPair(t,count));
        }

        public void Clear() {
            if (list != null) {
                list.Clear();
            } else {
                list = new List<ThingPair>();
            }
        }

        public void ExposeData() {
            Scribe_Collections.Look(ref list,"list",LookMode.Deep);
        }
    }
}
