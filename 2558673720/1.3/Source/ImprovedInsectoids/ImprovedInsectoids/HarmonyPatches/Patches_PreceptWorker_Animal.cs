using System.Linq;
using System.Collections.Generic;

using Verse;
using RimWorld;

using HarmonyLib;

namespace ImprovedInsectoids
{
    [HarmonyPatch(typeof(PreceptWorker_Animal))]
    public static class Patches_PreceptWorker_Animal
    {
        /// <summary>
        /// Patches ThingDefs to include insectoids as well.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PreceptWorker_Animal.ThingDefs), MethodType.Getter)]
        public static IEnumerable<PreceptThingChance> Postfix_ThingDefs(IEnumerable<PreceptThingChance> __result)
        {
            foreach (ThingDef thingDef in from def in DefDatabase<ThingDef>.AllDefs
                                          where (def.thingCategories?.Contains(ThingCategoryDefOf.Animals) ?? false) && !def.race.Dryad
                                          select def)
            {
                yield return new PreceptThingChance
                {
                    def = thingDef,
                    chance = 1f,
                };
            }
        }
    }
}
