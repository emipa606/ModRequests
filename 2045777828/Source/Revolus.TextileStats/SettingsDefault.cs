using RimWorld;
using System.Collections.Generic;

namespace Revolus.TextileStats {
    public class SettingsDefault {
        public static readonly List<string> StuffTableCategoryBuilding = new List<string> { "Stony", "Woody" };
        
        public static readonly List<string> StuffTableCategoryApparell = new List<string> { "Fabric", "Leathery", "Metallic" };
        
        public static readonly Dictionary<(StatKind, string), string> StatFormat = new Dictionary<(StatKind, string), string> {
            // common
            { (StatKind.Special, "Stat_Count"), "integer" },

            // stuff table
            { (StatKind.Base, "Beauty"), "integer" },
            { (StatKind.Base, "BluntDamageMultiplier"), "percent" },
            { (StatKind.Base, "ConstructionSpeedFactor"), "percent" },
            { (StatKind.Base, "DeteriorationRate"), "rate_per_day" },
            { (StatKind.Base, "Flammability"), "percent" },
            { (StatKind.Base, "MarketValue"), "silver" },
            { (StatKind.Base, "Mass"), "mass" },
            { (StatKind.Base, "MaxHitPoints"), "integer" },
            { (StatKind.Base, "RoyalFavorValue"), "decimal" },
            { (StatKind.Base, "SharpDamageMultiplier"), "percent" },
            { (StatKind.Base, "StuffPower_Armor_Blunt"), "percent" },
            { (StatKind.Base, "StuffPower_Armor_Heat"), "percent" },
            { (StatKind.Base, "StuffPower_Armor_Sharp"), "percent" },
            { (StatKind.Base, "StuffPower_Insulation_Cold"), "temperature" },
            { (StatKind.Base, "StuffPower_Insulation_Heat"), "temperature" },

            // building table
            { (StatKind.Special, "Stat_Building_uninstallWork"), "integer" },
            { (StatKind.Special, "Stat_Building_heatPerTickWhileWorking"), "temperature" },
            { (StatKind.Special, "Stat_Building_roofCollapseDamageMultiplier"), "percent" },
            { (StatKind.Special, "Stat_Building_bed_healPerDay"), "signed_decimal1" },
            { (StatKind.Special, "Stat_Building_bed_maxBodySize"), "centimeters" },
            { (StatKind.Special, "Stat_Building_nutritionCostPerDispense"), "decimal" },
            { (StatKind.Special, "Stat_Building_turretBurstWarmupTime"), "seconds" },
            { (StatKind.Special, "Stat_Building_turretBurstCooldownTime"), "seconds" },
            { (StatKind.Special, "Stat_Building_turretTopDrawSize"), "decimal" },  // TODO
            { (StatKind.Special, "Stat_Building_mineableNonMinedEfficiency"), "percent" },
            { (StatKind.Special, "Stat_Building_mineableDropChance"), "percent" },
            { (StatKind.Special, "Stat_Building_mineableScatterCommonality"), "percent" },
            { (StatKind.Special, "Stat_Building_trapPeacefulWildAnimalsSpringChanceFactor"), "percent" },
            { (StatKind.Special, "Stat_Building_unpoweredWorkTableWorkSpeedFactor"), "percent" },
        };

        public static string FactorFormat = "percent";
        public static string OffsetFormat = "signed_decimal1";
        public static string UnknownFormat = "decimal";
    }
}
