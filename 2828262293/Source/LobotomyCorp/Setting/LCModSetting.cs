using Verse;


namespace LobotomyCorp.Setting
{
    public class LCModSetting : ModSettings
    {

        public static LCModSetting Instance;

        public static bool isLCDamageAvailable = true;

        public static bool canDamageColonist = false;
        public static bool canDamageSameThingsInDifferentCell = false;

        public LCModSetting() {
        }

        public static void Rest()
        {
            isLCDamageAvailable = true;
            canDamageColonist = false;
            canDamageSameThingsInDifferentCell = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isLCDamageAvailable, "isLCDamageAvailable", true);
            Scribe_Values.Look(ref canDamageColonist, "canDamageColonist", false);
            Scribe_Values.Look(ref canDamageSameThingsInDifferentCell, "canDamageSameThingsInDifferentCell", false);
        }
    }
}
