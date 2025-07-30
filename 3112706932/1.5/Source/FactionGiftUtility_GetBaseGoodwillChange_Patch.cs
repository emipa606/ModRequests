using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GetBaseGoodwillChange")]
    public static class FactionGiftUtility_GetBaseGoodwillChange_Patch
    {
        public static void Postfix(ref float __result, Thing anyThing, int count, float singlePrice, Faction theirFaction)
        {
            if (anyThing?.def == IF_DefOf.IF_ImperialPropaganda)
            {
                Rand.PushState(anyThing.thingIDNumber);
                var amount = 8 * count;
                if (Rand.Chance(0.15f))
                {
                    amount = -amount;
                }
                __result = amount;
                Rand.PopState();
            }
        }
    }
}
