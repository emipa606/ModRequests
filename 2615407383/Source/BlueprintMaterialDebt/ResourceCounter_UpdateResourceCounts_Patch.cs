using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BlueprintMaterialDebt
{
    [HarmonyPatch(typeof(ResourceCounter), nameof(ResourceCounter.UpdateResourceCounts), new Type[0])]
    public static class ResourceCounter_UpdateResourceCounts_Patch
    {
        private static readonly HashSet<ThingDef> forcedVisible = new HashSet<ThingDef>();
        public static readonly Dictionary<ThingDef, int> neededAmounts = new Dictionary<ThingDef, int>();

        static void Postfix(Map ___map)
        {
            if (___map != Find.CurrentMap)
            {
                // only update for current map
                return;
            }

            Reset();

            foreach (var thing in ___map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint))
            {
                var blueprint = thing as Blueprint_Build;
                if (blueprint != null)
                {
                    if (blueprint.IsForbidden(Faction.OfPlayer) && !BlueprintMaterialDebt.IncludeForbidden)
                    {
                        continue;
                    }

                    foreach (var material in blueprint.MaterialsNeeded())
                    {
                        if (neededAmounts.ContainsKey(material.thingDef))
                        {
                            neededAmounts[material.thingDef] += material.count;
                        }
                        else
                        {
                            neededAmounts.Add(material.thingDef, material.count);
                        }
                    }
                }
            }

            foreach (var thing in ___map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame))
            {
                var frame = thing as Frame;
                if (frame != null)
                {
                    if (frame.IsForbidden(Faction.OfPlayer) && !BlueprintMaterialDebt.IncludeForbidden)
                    {
                        continue;
                    }

                    foreach (var material in frame.MaterialsNeeded())
                    {
                        if (neededAmounts.ContainsKey(material.thingDef))
                        {
                            neededAmounts[material.thingDef] += material.count;
                        }
                        else
                        {
                            neededAmounts.Add(material.thingDef, material.count);
                        }
                    }
                }
            }

            foreach (var material in neededAmounts.Keys)
            {
                if (!material.resourceReadoutAlwaysShow)
                {
                    material.resourceReadoutAlwaysShow = true;

                    forcedVisible.Add(material);
                }
            }
        }

        internal static void Reset()
        {
            foreach (var thing in forcedVisible)
            {
                thing.resourceReadoutAlwaysShow = false;
            }

            forcedVisible.Clear();
            neededAmounts.Clear();
        }
    }
}
