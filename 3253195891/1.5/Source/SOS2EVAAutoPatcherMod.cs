using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SOS2EVAAutoPatcher
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.IsSuitableAsEva())
                {
                    if ((thingDef.apparel.tags?.Contains("EVA") ?? false) is false)
                    {
                        thingDef.apparel.tags ??= new List<string>();
                        thingDef.apparel.tags.Add("EVA");
                        Log.Message("Added EVA tag to " + thingDef.label + " from " + (thingDef.modContentPack?.Name ?? "unknown mod"));
                    }

                    thingDef.SetStatBaseValue(StatDefOf.Insulation_Cold, 100);
                    Log.Message("Added cold resist to " + thingDef.label + " from " + (thingDef.modContentPack?.Name ?? "unknown mod"));

                    float decompResist = 0f;

                    if (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                    {
                        decompResist = .75f;
  //                      thingDef.SetStatBaseValue(SaveOurShip2.ResourceBank.StatDefOf.DecompressionResistance, .75f);
  //                      Log.Message("Added Decompression resist to body item " + thingDef.label + " from " + (thingDef.modContentPack?.Name ?? "unknown mod"));
                    }
                    else if (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                    {
                        decompResist = .25f;
                        //                      thingDef.SetStatBaseValue(SaveOurShip2.ResourceBank.StatDefOf.DecompressionResistance, .25f);
                        //                      thingDef.SetStatBaseValue(SaveOurShip2.ResourceBank.StatDefOf.HypoxiaResistance, 1);
                        //                      Log.Message("Added Hypoxia and Decompression resist to head item " + thingDef.label + " from " + (thingDef.modContentPack?.Name ?? "unknown mod"));

                        thingDef.equippedStatOffsets ??= new List<StatModifier>();
                        if (thingDef.equippedStatOffsets.Exists(x => x.stat == SaveOurShip2.ResourceBank.StatDefOf.HypoxiaResistance))
                        {
                            StatModifier entry = thingDef.equippedStatOffsets.Find(x => x.stat == SaveOurShip2.ResourceBank.StatDefOf.HypoxiaResistance);
                            entry.value = 1;
                        }
                        else
                        {
                            thingDef.equippedStatOffsets.Add(new StatModifier
                            {
                                stat = SaveOurShip2.ResourceBank.StatDefOf.HypoxiaResistance,
                                value = 1
                            });
                        }
                    }

                    thingDef.equippedStatOffsets ??= new List<StatModifier>();
                    if (thingDef.equippedStatOffsets.Exists(x => x.stat == SaveOurShip2.ResourceBank.StatDefOf.DecompressionResistance))
                    {
                        StatModifier entry = thingDef.equippedStatOffsets.Find(x => x.stat == SaveOurShip2.ResourceBank.StatDefOf.DecompressionResistance);
                        entry.value = decompResist;
                    }
                    else
                    {
                        thingDef.equippedStatOffsets.Add(new StatModifier
                        {
                            stat = SaveOurShip2.ResourceBank.StatDefOf.DecompressionResistance,
                            value = decompResist
                        });
                    }



                    thingDef.equippedStatOffsets ??= new List<StatModifier>();
                    if (thingDef.equippedStatOffsets.Exists(x => x.stat == StatDefOf.ToxicResistance))
                    {
                        StatModifier entry = thingDef.equippedStatOffsets.Find(x => x.stat == StatDefOf.ToxicResistance);
                        entry.value = 1;
                    }
                    else
                    {
                        thingDef.equippedStatOffsets.Add(new StatModifier
                        {
                            stat = StatDefOf.ToxicResistance,
                            value = 1
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
                && ((thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && thingDef.apparel.LastLayer == ApparelLayerDefOf.Shell) ||
                (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)) 
                || (thingDef.apparel.tags?.Contains("MandalorianApparel") ?? false)
               // && ( (thingDef.tradeTags?.Contains("Armor") ?? false) 
               // && thingDef.apparel.shellCoversHead
               // && (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes)) 
               // (thingDef.apparel.bodyParts.Contains(BodyPartDefOf.Nose)) &&
               // (thingDef.apparel.bodyPartGroups.Contains(BodyPartDefOf.Jaw))
               ))
             {
                if (thingDef.tradeTags != null && thingDef.tradeTags.Contains("PsychicApparel"))
                {
                    return false;
                }
                Log.Message("SOS2 EVA Autopatcher evaluating " + thingDef.label + " from "
                        + (thingDef.modContentPack?.Name ?? "unknown mod") + " for EVA");
                bool madeOutMetallic = thingDef.costList == null && thingDef.stuffCategories == null;
                if (thingDef.costList != null && thingDef.costList.Exists(x => x.thingDef.stuffProps?.categories?.Any(x => x == StuffCategoryDefOf.Metallic) ?? false))
                {
                    madeOutMetallic = true;
                }
                if (thingDef.stuffCategories != null && thingDef.stuffCategories.Exists(x => x == StuffCategoryDefOf.Metallic))
                {
                    madeOutMetallic = true;
                }
                if (madeOutMetallic)
                    Log.Message("SOS2 EVA Autopatcher accepted " + thingDef.label + " from "
                       + (thingDef.modContentPack?.Name ?? "unknown mod") + " as  metallic and valid for EVA");
                else
                    Log.Message("SOS2 EVA Autopatcher denied " + thingDef.label + " from "
                       + (thingDef.modContentPack?.Name ?? "unknown mod") + " as not metallic and not valid for EVA");



                return madeOutMetallic;
            }
            return false;
        }
    }
}
