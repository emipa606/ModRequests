using HarmonyLib;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [HarmonyPatch(typeof(TradeUtility), "GetPricePlayerSell")]
    public static class Patch_TradeUtility_GetPricePlayerSell
    {
        public static void Postfix(ref float __result, Thing thing, TradeCurrency currency)
        {
            if (currency == TradeCurrency.Silver && thing.def == BankDefOf.BankNote)
            {
                __result = thing.MarketValue;
            }
        }
    }
}