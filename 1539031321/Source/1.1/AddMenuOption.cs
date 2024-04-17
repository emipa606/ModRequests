using System;
using Verse;

namespace StatueOfColonist {
    public struct AddMenuOption {
        public ThingDef thingDef;

        public Action method;

        public AddMenuOption(ThingDef thingDef, Action method) {
            this.thingDef = thingDef;
            this.method = method;
        }
    }
}
