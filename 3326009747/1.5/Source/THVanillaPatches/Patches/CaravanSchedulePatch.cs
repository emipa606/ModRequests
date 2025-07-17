using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using THVanillaPatches.HelperThings;
using UnityEngine;
using Verse;

namespace THVanillaPatches.Patches
{
    public class CaravanSchedulePatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        protected override List<PatchInfo> Patches => new List<PatchInfo>()
        {
            new PatchInfo(AccessTools.PropertyGetter(typeof(Caravan), nameof(Caravan.NightResting)), Prefix: new HarmonyMethod(GetType(), nameof(NewCaravanNightResting))),
            new PatchInfo(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetInspectString)), Postfix: new HarmonyMethod(GetType(), nameof(AppendNocturnalDescription)))
        };

        private static void AppendNocturnalDescription(Caravan __instance, ref string __result)
        {
            if (__instance.IsMostlyNocturnalPawns())
            {
                __result += "THVP.CaravanNocturnal".Translate();
            }
        }
        
        private static bool NewCaravanNightResting(Caravan __instance, ref bool __result)
        {
            __result = __instance.Spawned && __instance.needs.AnyPawnsNeedRest && (!__instance.pather.Moving || __instance.pather.nextTile != __instance.pather.Destination || !Caravan_PathFollower.IsValidFinalPushDestination(__instance.pather.Destination) || Mathf.CeilToInt(__instance.pather.nextTileCostLeft / 1f) > 10000) && NightableCaravanNightRestUtility.RestingNowAt(__instance.Tile, __instance.IsMostlyNocturnalPawns());
            return false;
        }
    }
}