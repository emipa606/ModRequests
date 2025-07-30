using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ImperialFunctionality
{
    public class RoyalTitlePermitWorker_DropResources_RandomAmount : RoyalTitlePermitWorker_DropResources
    {
        [HarmonyPatch(typeof(RoyalTitlePermitWorker_DropResources), nameof(CallResourcesToCaravan))]
        public static class RoyalTitlePermitWorker_DropResources_CallResourcesToCaravan_Patch
        {
            public static bool Prefix(RoyalTitlePermitWorker_DropResources __instance, Pawn caller, Faction faction, bool free)
            {
                if (__instance is RoyalTitlePermitWorker_DropResources_RandomAmount)
                {
                    CallResourcesToCaravan(__instance, caller, faction, free);
                    return false;
                }
                return true;
            }

            private static void CallResourcesToCaravan(RoyalTitlePermitWorker_DropResources __instance, Pawn caller, Faction faction, bool free)
            {
                Caravan caravan = caller.GetCaravan();
                foreach (var item in __instance.def.royalAid.itemsToDrop)
                {
                    var stack = Rand.RangeInclusive(1, item.count);
                    while (stack > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(item.thingDef);
                        thing.stackCount = Mathf.Min(stack, item.thingDef.stackLimit);
                        stack -= thing.stackCount;
                        CaravanInventoryUtility.GiveThing(caravan, thing);
                    }
                }
                Messages.Message("MessagePermitTransportDropCaravan".Translate(faction.Named("FACTION"), caller.Named("PAWN")), caravan, MessageTypeDefOf.NeutralEvent);
                caller.royalty.GetPermit(__instance.def, faction).Notify_Used();
                if (!free)
                {
                    caller.royalty.TryRemoveFavor(faction, __instance.def.royalAid.favorCost);
                }
            }
        }

        [HarmonyPatch(typeof(RoyalTitlePermitWorker_DropResources), nameof(CallResources))]
        public static class RoyalTitlePermitWorker_DropResources_CallResources_Patch
        {
            public static bool Prefix(RoyalTitlePermitWorker_DropResources __instance, IntVec3 cell)
            {
                if (__instance is RoyalTitlePermitWorker_DropResources_RandomAmount)
                {
                    CallResources(__instance, cell);
                    return false;
                }
                return true;
            }

            private static void CallResources(RoyalTitlePermitWorker_DropResources __instance, IntVec3 cell)
            {
                List<Thing> list = new List<Thing>();
                foreach (var item in __instance.def.royalAid.itemsToDrop)
                {
                    var stack = Rand.RangeInclusive(1, item.count);
                    while (stack > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(item.thingDef);
                        thing.stackCount = Mathf.Min(stack, item.thingDef.stackLimit);
                        stack -= thing.stackCount;
                        list.Add(thing);
                    }
                }
                if (list.Any())
                {
                    ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                    activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(list);
                    DropPodUtility.MakeDropPodAt(cell, __instance.map, activeDropPodInfo);
                    Messages.Message("MessagePermitTransportDrop".Translate(__instance.faction.Named("FACTION")), new LookTargets(cell, __instance.map), MessageTypeDefOf.NeutralEvent);
                    __instance.caller.royalty.GetPermit(__instance.def, __instance.faction).Notify_Used();
                    if (!__instance.free)
                    {
                        __instance.caller.royalty.TryRemoveFavor(__instance.faction, __instance.def.royalAid.favorCost);
                    }
                }
            }
        }
    }
}
