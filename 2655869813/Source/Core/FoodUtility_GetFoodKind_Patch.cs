using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsVanillaExpandedPatches
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetFoodKind), new[] { typeof(ThingDef) })]
    static class FoodUtility_GetFoodKind_Patch
    {
        public static FoodKind Postfix(FoodKind __result, ThingDef foodDef)
        {
            if (foodDef.defName == "Tofu" && BenLubarsVanillaExpandedPatches.tofuIsNonMeat)
            {
                return FoodKind.NonMeat;
            }

            return __result;
        }
    }
}
