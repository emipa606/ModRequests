using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace SSC
{
    [HarmonyPatch(typeof(Thing), "SpecialDisplayStats")]
    public static class ShowStuffCategoriesPatch
    {
        static void Postfix(ref IEnumerable<StatDrawEntry> __result, Thing __instance)
        {
            if (!__instance.def.stuffCategories.EnumerableNullOrEmpty())
            {
                var stuffCategoryStrings = __instance.def.stuffCategories.Select(sc => sc.ToString()).ToArray();
                string stringList = stuffCategoryStrings[0];
                for (int i = 1; i < stuffCategoryStrings.Length; i++)
                    stringList += ", " + stuffCategoryStrings[i];
                Log.Message("Item has the following stuff categories in its def: " + stringList);
                __result = __result.AddItem(new StatDrawEntry(StatCategoryDefOf.Basics, "Stuff categories used", stringList, "The categories of stuff used to make this.", 0));
            }
            if (__instance.def.stuffProps != null)
            {
                var stuffCategoryStrings = __instance.def.stuffProps.categories.Select(sc => sc.ToString()).ToArray();
                string stringList = stuffCategoryStrings[0];
                for (int i = 1; i < stuffCategoryStrings.Length; i++)
                    stringList += ", " + stuffCategoryStrings[i];
                __result = __result.AddItem(new StatDrawEntry(StatCategoryDefOf.Basics, "Stuff categories", stringList, "The categories of stuff that this falls under.", 0));
            }
        }
    }
}