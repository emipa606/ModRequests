using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

using HarmonyLib;

namespace ButcheryCountsMeatTypes
{

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("MrHacky.ButcheryCountsMeatTypes");

            harmony.Patch(
                AccessTools.Method(typeof(Verse.RecipeWorkerCounter_ButcherAnimals), "CountProducts"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod("CountProducts_Postfix"))
            );
        }

        public static int CountProducts_Postfix(int prev, Bill_Production bill)
        {
            // Sanity/compatibility check
            if (bill.recipe.specialProducts.Contains(Verse.SpecialProductType.Butchery))
            {
                // Find the allowed meat types based on allowed corpse types in ingredient filter
                HashSet<ThingDef> allowedMeatDefs = new HashSet<ThingDef>();
                foreach (ThingDef allowedThingDef in bill.ingredientFilter.AllowedThingDefs)
                {
                    if (allowedThingDef.ingestible != null && allowedThingDef.ingestible.sourceDef != null)
                    {
                        RaceProperties race = allowedThingDef.ingestible.sourceDef.race;
                        if (race != null && race.meatDef != null)
                            allowedMeatDefs.Add(race.meatDef);
                    }
                }

                // Substract from the normal result any meat types not allowed
                foreach (ThingDef meatDef in ThingCategoryDefOf.MeatRaw.childThingDefs)
                    if (!allowedMeatDefs.Contains(meatDef))
                        prev -= bill.Map.resourceCounter.GetCount(meatDef);
            }

            // return (possibly adjusted) result
            return prev;
        }
    }
}