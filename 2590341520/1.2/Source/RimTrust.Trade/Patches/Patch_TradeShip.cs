using HarmonyLib;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [HarmonyPatch(typeof(TradeShip), "GenerateThings")]
    public static class Patch_TradeShip
    {
        public static void Postfix(TradeShip __instance)
        {
            Thing thing = ThingMaker.MakeThing(BankDefOf.BankNote);
            thing.stackCount = Rand.Range(8, 16);
            ((ThingOwner)AccessTools.Field(typeof(TradeShip), "things").GetValue(__instance)).TryAdd(thing);
        }
    }
}