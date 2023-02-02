using RimWorld;
using Verse;

namespace SSTHDweapons
{
    public class ModSettings_SSTHDweapons : ModSettings
    {
        public static bool weaponsalwaysvisible = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref weaponsalwaysvisible, "weaponsalwaysvisible", true);
            base.ExposeData();
        }
    }
}