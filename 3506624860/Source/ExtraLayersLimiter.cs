using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraLayersLimiter
{
    [StaticConstructorOnStartup]
    public static class ExtraLayersLimiterLoader
    {
        static ExtraLayersLimiterLoader()
        {
            var harmony = new Harmony("Clouds.ExtraLayers");
            harmony.PatchAll();
            Log.Message("[Cloud's Extra Layers] Patch applied successfully.");
        }
    }

    [HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.CanWearTogether))]
    public static class Patch_ApparelUtility_CanWearTogether
    {
        // Manual mutual-exclusion rules
        private static readonly List<(string, string)> ExclusivePairs = new List<(string, string)>
        {
            ("boots", "shoes"),
            ("toolbelt", "belt"),
            ("broadwrap", "broadwrap"),
            ("veil", "veil"),
            ("garland", "garland"),
            ("mask", "mask"),
            ("pants", "trousers"),
            ("helmet", "helmet"),
            ("gauntlet", "gauntlet"),
            ("dress", "dress"),
            ("gloves", "gloves"),
            ("shorts", "pants"),
            ("shorts", "trousers"),
            ("coat", "jacket"),
            ("robe", "coat"),
            ("coat", "jacket"),
            ("robe", "jacket"),
            ("panty", "bottom"),
            ("armor", "armor"),
            ("armor", "cuirass"),
            ("backpack", "backpack"),
            ("jumpsuit", "jumpsuit"),
            ("crown", "crown"),
            ("crown", "hat"),
            ("hood", "hood"),
            ("collar", "collar"),
            ("shield", "shield"),
            ("belt", "belt"),
            ("vest", "vest"),
            ("hood", "hat"),
            ("fedora", "hat"),
            ("cap", "hat"),
            ("cap", "cap"),
            ("beret", "hat"),
            ("beret", "cap"),
            ("coronet", "crown"),
            ("coronet", "hat"),
            ("coronet", "cap"),
            ("crown", "cap"),
            ("fez", "hat"),
            ("fez", "cap"),
            ("crown", "fez"),
            ("coronet", "fez"),
            ("glasses", "glasses"),
            ("monocle", "glasses"),
            ("blouse", "shirt"),
            ("shirt", "shirt"),
            ("hoodie", "jacket"),
            ("blouse", "tunic"),
            ("tunic", "shirt"),
            ("hardhat", "cap"),
            ("hardhat", "hat"),
            ("hardhat", "crown"),
            ("hardhat", "coronet"),
            ("hardhat", "fez"),
            ("hardhat", "fedora"),
            ("shield", "buckler"),
            ("boots", "boots"),
            ("hat", "hat") // Optional: remove if stacking hats is okay
        };

        // Determines if two defs are duplicates or mutually exclusive
        private static bool IsSameOrExclusive(ThingDef A, ThingDef B)
        {
            string a = A.defName.ToLowerInvariant();
            string b = B.defName.ToLowerInvariant();

            if (a == b)
                return true;

            foreach (var (x, y) in ExclusivePairs)
            {
                if ((a.Contains(x) && b.Contains(y)) || (a.Contains(y) && b.Contains(x)))
                    return true;
            }

            return false;
        }

        public static bool Prefix(ThingDef A, ThingDef B, BodyDef body, ref bool __result)
        {
            if (A?.apparel == null || B?.apparel == null)
                return true;

            // If they don't share a layer, allow it
            if (!A.apparel.layers.Any(layer => B.apparel.layers.Contains(layer)))
                return true;

            // If they don't share any body parts, allow it
            if (!A.apparel.bodyPartGroups.Any(part => B.apparel.bodyPartGroups.Contains(part)))
                return true;

            // If same or manually exclusive, block it
            if (IsSameOrExclusive(A, B))
            {
                __result = false;
                return false;
            }

            // Otherwise, allow it
            __result = true;
            return false;
        }
    }
}
