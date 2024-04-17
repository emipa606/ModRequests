using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WhereIsMyWeapon {
    public class WhereIsMyWeaponSettings : ModSettings {
        public bool reequip = true;
        public bool retake = true;
        public int searchDistance = 50;

        public override void ExposeData() {
            Scribe_Values.Look(ref this.reequip, "reequip",true);
            Scribe_Values.Look(ref this.retake, "retake", true);
            Scribe_Values.Look(ref this.searchDistance, "searchDistance", 50);
        }
    }
}
