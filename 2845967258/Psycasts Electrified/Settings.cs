using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psycasts_Electrified
{
    public class Settings : ModSettings
    {
        public static float earChargeRate = 60f;
        public static float earMaxCharge = 6000f;
        public static float earPowerUsage = 3f;
        public static float electricalOverloadCost = 50f;
        public static float electricalSuperloadIncreasePerLevel = 1000f;
        public static float electricalSuperloadBaseCost = 1000f;
        public static float neuralHeatsink = 25f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref earChargeRate, "earChargeRate", 60f);
            Scribe_Values.Look(ref earMaxCharge, "earMaxCharge", 6000f);
            Scribe_Values.Look(ref earPowerUsage, "earPowerUsage", 3f);
            Scribe_Values.Look(ref electricalOverloadCost, "electricalOverloadCost", 50f);
            Scribe_Values.Look(ref electricalSuperloadIncreasePerLevel, "electricalSuperloadIncreasePerLevel", 1000f);
            Scribe_Values.Look(ref electricalSuperloadBaseCost, "electricalSuperlaodBaseCost", 1000f);
            Scribe_Values.Look(ref neuralHeatsink, "neuralHeatsink", 25f);
        }
    }
}
