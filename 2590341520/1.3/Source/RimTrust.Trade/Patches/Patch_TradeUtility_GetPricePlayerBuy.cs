using HarmonyLib;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [HarmonyPatch(typeof(TradeUtility), "GetPricePlayerBuy")]
    public static class Patch_TradeUtility_GetPricePlayerBuy
    {
        public static void Postfix(ref float __result, Thing thing)
        {
            if (thing.def == BankDefOf.BankNote)
            {
                __result = thing.MarketValue;
            }
        }
    }
}