using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SOS2EVAAutoPatcher
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.IsSuitableAsEva())
                {
                    if ((thingDef.apparel.tags?.Contains("EVA") ?? false) is false)
                    {
                        thingDef.apparel.tags ??= new List<string>();
                        thingDef.apparel.tags.Add("EVA");
                    }
                    thingDef.SetStatBaseValue(StatDefOf.Insulation_Cold, 100);
                    if (thingDef.equippedStatOffsets is null)
                    {
                        thingDef.equippedStatOffsets = new List<StatModifier>();
                    }
                    if (thingDef.equippedStatOffsets.Exists(x => x.stat == StatDefOf.ToxicSensitivity))
                    {
                        var entry = thingDef.equippedStatOffsets.Find(x => x.stat == StatDefOf.ToxicSensitivity);
                        entry.value = -1;
                    }
                    else
                    {
                        thingDef.equippedStatOffsets.Add(new StatModifier
                        {
                            stat = StatDefOf.ToxicSensitivity,
                            value = -1
                        });
                    }

                    Log.Message("SOS2 EVA Autopatcher patched " + thingDef.label + " from "
                        + (thingDef.modContentPack?.Name ?? "unknown mod") + " to be usable as an EVA apparel.");
                }
            }
        }

        private static bool IsSuitableAsEva(this ThingDef thingDef)
        {
            if (thingDef.IsApparel && (thingDef.techLevel >= TechLevel.Spacer
                || (thingDef.tradeTags?.Contains("Warcasket") ?? false))
                && thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso)
                && thingDef.apparel.LastLayer == ApparelLayerDefOf.Shell)
            {
                if (thingDef.tradeTags != null && thingDef.tradeTags.Contains("PsychicApparel"))
                {
                    return false;
                }
                bool madeOutMetallic = thingDef.costList != null || thingDef.stuffCategories != null ? false : true;
                if (thingDef.costList != null && thingDef.costList.Exists(x => x.thingDef.stuffProps?.categories?.Any(x => x == StuffCategoryDefOf.Metallic) ?? false))
                {
                    madeOutMetallic = true;
                }
                if (thingDef.stuffCategories != null && thingDef.stuffCategories.Exists(x => x == StuffCategoryDefOf.Metallic))
                {
                    madeOutMetallic = true;
                }
                if (!madeOutMetallic)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
