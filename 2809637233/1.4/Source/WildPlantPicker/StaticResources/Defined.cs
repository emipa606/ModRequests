using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using WildPlantPicker.DataStore;
using WildPlantPicker.DataStore.Entities;

namespace WildPlantPicker.StaticResources
{
    [StaticConstructorOnStartup]
    internal static class Defined
    {
        internal const string MOD_ABBREVIATED_NAME = "WPP";
        internal static readonly int TECH_LEVEL_MAX = Enum.GetValues(typeof(TechLevel)).Cast<byte>().Max();
        internal static readonly int QUALITY_CATEGORY_MAX = Enum.GetValues(typeof(QualityCategory)).Cast<byte>().Max();
        internal static readonly int ALWAYS_FREQUENCY_MAX = Enum.GetValues(typeof(AlwaysFrequency)).Cast<byte>().Max();
        internal static readonly int HARVEST_THRESHOLD_MAX = Enum.GetValues(typeof(HarvestThreshold)).Cast<byte>().Max();
        internal static readonly Dictionary<AlwaysFrequency, int> FREQUENCY_VALUES = new Dictionary<AlwaysFrequency, int>() {
            { AlwaysFrequency.Low, 600 },
            { AlwaysFrequency.Middle, 300 },
            { AlwaysFrequency.High, 100 },
        };

        internal static readonly Regex INT_MAX_REGEX = new Regex("^[0-9]+$");

        internal static readonly HashSet<ThingDef> ALL_PLANTS;
        internal static readonly HashSet<ThingDef> HARVESTABLE_PLANTS;
        internal static readonly HashSet<ThingDef> PURE_WILD_PLANTS;
        internal static readonly HashSet<ThingDef> IMMUTABLE_PLANTS;

        static Defined()
        {
            ALL_PLANTS = new HashSet<ThingDef>();
            PURE_WILD_PLANTS = new HashSet<ThingDef>();
            HARVESTABLE_PLANTS = new HashSet<ThingDef>();
            IMMUTABLE_PLANTS = new HashSet<ThingDef>();
            foreach (ThingDef plant in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Plant && x.plant?.harvestedThingDef != null))
            {
                ALL_PLANTS.Add(plant);
                if (plant.plant.harvestedThingDef != null)
                {
                    HARVESTABLE_PLANTS.Add(plant);
                    if (!plant.plant.Sowable)
                    {
                        PURE_WILD_PLANTS.Add(plant);
                        if (plant == ThingDefOf.Plant_Ambrosia)
                        {
                            IMMUTABLE_PLANTS.Add(plant);
                        }
                    }
                }
            }
        }

    }
}
