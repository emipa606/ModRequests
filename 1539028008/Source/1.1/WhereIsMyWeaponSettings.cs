using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WhereIsMyWeapon {
    public class WhereIsMyWeaponSettings : ModSettings {
        public enum RetakeTrigger {
            Undowned,
            FullyHealed,
        }

        public bool reequip = true;
        public bool retake = true;
        public int searchDistance = 50;
        public RetakeTrigger retakeTrigger = RetakeTrigger.Undowned;

        public override void ExposeData() {
            Scribe_Values.Look(ref this.reequip, "reequip",true);
            Scribe_Values.Look(ref this.retake, "retake", true);
            Scribe_Values.Look(ref this.searchDistance, "searchDistance", 50);
            Scribe_Values.Look(ref this.retakeTrigger, "retakeTrigger", RetakeTrigger.Undowned);
        }
    }
}
