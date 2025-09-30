using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SettlementQuests
{
    [HarmonyPatch(typeof(QuestNode_GetSiteTile), "TryFindTile")]
    public static class QuestNode_GetSiteTile_TryFindTile_Patch
    {
        public static WorldObject worldObject;
        public static bool Prefix(ref bool __result, QuestNode_GetSiteTile __instance, Slate slate, ref int tile)
        {
            if (worldObject != null && TryFindTile(worldObject, slate, __instance.preferCloserTiles, __instance.allowCaravans, __instance.clampRangeBySiteParts, __instance.sitePartDefs, out tile))
            {
                __result = true;
                return false;
            }
            return true;
        }

        private static bool TryFindTile(WorldObject worldObject, Slate slate, SlateRef<bool> preferCloserTiles, SlateRef<bool> allowCaravans, SlateRef<bool?> clampRangeBySiteParts, SlateRef<IEnumerable<SitePartDef>> sitePartDefs, out int tile)
        {
            int nearThisTile = worldObject.Tile;
            int num = int.MaxValue;
            bool? value = clampRangeBySiteParts.GetValue(slate);
            if (value.HasValue && value.Value && sitePartDefs != null)
            {
                try
                {
                    foreach (SitePartDef item in sitePartDefs.GetValue(slate))
                    {
                        if (item.conditionCauserDef != null)
                        {
                            num = Mathf.Min(num, item.conditionCauserDef.GetCompProperties<CompProperties_CausesGameCondition>().worldRange);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            if (!slate.TryGet("siteDistRange", out IntRange var))
            {
                var = new IntRange(7, Mathf.Min(27, num));
            }
            else if (num != int.MaxValue)
            {
                var = new IntRange(Mathf.Min(var.min, num), Mathf.Min(var.max, num));
            }
            var tileMode = preferCloserTiles.GetValue(slate) ? TileFinderMode.Near : TileFinderMode.Random;
            return TileFinder.TryFindNewSiteTile(out tile, var.min, var.max, allowCaravans.GetValue(slate), tileMode, nearThisTile);
        }
    }
}
