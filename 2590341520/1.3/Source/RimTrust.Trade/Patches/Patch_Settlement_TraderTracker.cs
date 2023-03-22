using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimTrust.Trade
{
    [HarmonyPatch(typeof(Settlement_TraderTracker), "RegenerateStock")]
    public static class Patch_Settlement_TraderTracker
    {
        public static void Postfix(Settlement_TraderTracker __instance)
        {
            Thing thing = ThingMaker.MakeThing(BankDefOf.BankNote);
            int min = 0;
            int max = 0;
            switch (__instance.settlement.Faction.def.techLevel)
            {
                case TechLevel.Neolithic:
                    min = 0;
                    max = 5;
                    break;

                case TechLevel.Medieval:
                    min = 3;
                    max = 7;
                    break;

                case TechLevel.Industrial:
                    min = 4;
                    max = 11;
                    break;

                case TechLevel.Spacer:
                case TechLevel.Ultra:
                    min = 7;
                    max = 14;
                    break;

                case TechLevel.Archotech:
                    min = 9;
                    max = 16;
                    break;
            }
            thing.stackCount = Rand.Range(min, max);
            ((ThingOwner<Thing>)AccessTools.Field(typeof(Settlement_TraderTracker), "stock").GetValue(__instance)).TryAdd(thing);
        }
           
    }
}