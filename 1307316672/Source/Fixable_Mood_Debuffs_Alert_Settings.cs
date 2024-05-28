using Verse;

namespace Fixable_Mood_Debuffs_Alert
{
    class Fixable_Mood_Debuffs_Alert_Settings : ModSettings
    {
        public bool alertOnWrongMaster = true;
        public bool alertOnNightOwlInDay = true;
        public bool alertOnClothedNudist = true;
        public bool alertOnProsthophile = true;
        public bool alertOnBedroom = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref alertOnWrongMaster, "alertOnWrongMaster", true);
            Scribe_Values.Look(ref alertOnNightOwlInDay, "alertOnNightOwlInDay", true);
            Scribe_Values.Look(ref alertOnClothedNudist, "alertOnClothedNudist", true);
            Scribe_Values.Look(ref alertOnBedroom, "alertOnBedroom", true);
            Scribe_Values.Look(ref alertOnProsthophile, "alertOnProsthophile", true);
        }
    }
}

