using Verse;

namespace ArchotechPlus
{
    public class ArchotechPlusSettings : ModSettings
    {
        public static bool RegeneratorDeAge = true;
        public static int TargetAge = 30;
        public static int MaxHealingCharges = 1;
        public static bool RegeneratorResurrects = true;
        public static int MaxResurrectionCharges = 1;
        public static IntRange HealingRange = new IntRange(120, 240);
        public static IntRange ResurrectionRange =  new IntRange(12, 96);

        public override void ExposeData()
        {
            Scribe_Values.Look(ref RegeneratorDeAge, "regeneratorDeAge", true);
            Scribe_Values.Look(ref TargetAge, "targetAge", 28);
            Scribe_Values.Look(ref MaxHealingCharges, "maxHealingCharges", 1);
            Scribe_Values.Look(ref RegeneratorResurrects, "regeneratorResurrects", true);
            Scribe_Values.Look(ref MaxResurrectionCharges, "maxResurrectionCharges", 1);
            Scribe_Values.Look(ref HealingRange, "healingRange", new IntRange(120, 240));
            Scribe_Values.Look(ref ResurrectionRange, "resurrectionRange", new IntRange(12, 96));
            base.ExposeData();
        }
    }
    
    
}