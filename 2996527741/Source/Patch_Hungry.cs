using HarmonyLib;
using RimWorld;
using Verse;

namespace Entomophagy
{
    [HarmonyPatch(typeof(Need_Food))]
    class Patch_Hungry
    {/*
        [HarmonyPostfix]
        [HarmonyPatch("get_PercentageThreshHungry")]
        public static float Postfix_Hungry(float __result, Need_Food __instance, Pawn ___pawn)
        {
            if (___pawn.needs.mood != null && ___pawn.story.traits.HasTrait(TraitDef.Named("VT_Gourmet")))
            {
                return __result * 1.4f;
            }
            return __result;
        }

        [HarmonyPatch("get_PercentageThreshUrgentlyHungry")]
        [HarmonyPostfix]
        public static float Postfix_HungryUrgent(float __result, Need_Food __instance, Pawn ___pawn)
        {
            if (___pawn.needs.mood != null && ___pawn.story.traits.HasTrait(TraitDef.Named("VT_Gourmet")))
            {
                return __result * 1.4f;
            }
            return __result;
        } */
    }
}
