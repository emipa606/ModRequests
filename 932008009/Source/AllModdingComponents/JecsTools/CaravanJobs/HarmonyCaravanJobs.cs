﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace JecsTools
{
    [StaticConstructorOnStartup]
    internal static class HarmonyCaravanPatches
    {
        static HarmonyCaravanPatches()
        {
            var harmony = new Harmony("jecstools.jecrell.caravanjobs");
            var type = typeof(HarmonyCaravanPatches);

            harmony.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetInspectString)),
                postfix: new HarmonyMethod(type, nameof(GetInspectString_Jobs)));
            harmony.Patch(AccessTools.Method(typeof(WorldSelector), "AutoOrderToTileNow"),
                postfix: new HarmonyMethod(type, nameof(AutoOrderToTileNow_Jobs)));
            harmony.Patch(AccessTools.Method(typeof(Caravan), nameof(Caravan.GetGizmos)),
                postfix: new HarmonyMethod(type, nameof(GetGizmos_Jobs)));
            harmony.Patch(AccessTools.Method(typeof(WorldSelector), nameof(WorldSelector.SelectableObjectsUnderMouse),
                    new[] { typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType() }),
                postfix: new HarmonyMethod(type, nameof(SelectableObjectsUnderMouse_InvisHandler)));
        }

        // RimWorld.Planet.WorldSelector
        public static void SelectableObjectsUnderMouse_InvisHandler(ref IEnumerable<WorldObject> __result)
        {
            static bool NotSelectableNow(WorldObject o) => !o.SelectableNow;
            if (__result is List<WorldObject> list)
                list.RemoveAll(NotSelectableNow);
            else
                __result = __result.Where(NotSelectableNow);
        }

        // RimWorld.Planet.Caravan
        public static void GetGizmos_Jobs(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.IsPlayerControlled)
            {
                var curTile = Find.WorldGrid[__instance.Tile];
                var caravanJobGiver = CaravanJobsUtility.GetCaravanJobGiver();
                if (caravanJobGiver.CurJob(__instance) != null)
                    __result = __result.Append(new Command_Action
                    {
                        defaultLabel = "CommandCancelConstructionLabel".Translate(),
                        defaultDesc = "CommandClearPrioritizedWorkDesc".Translate(),
                        icon = TexCommand.ClearPrioritizedWork,
                        action = () => caravanJobGiver.Tracker(__instance).StopAll(),
                    });
            }
        }

        // RimWorld.Planet.WorldSelector
        public static void AutoOrderToTileNow_Jobs(Caravan c)
        {
            CaravanJobsUtility.GetCaravanJobGiver().Tracker(c).StopAll();
        }

        // RimWorld.Planet.Caravan

        public static void GetInspectString_Jobs(Caravan __instance, ref string __result)
        {
            if (CaravanJobsUtility.GetCaravanJobGiver().Tracker(__instance)?.curDriver?.GetReport() is string s &&
                __result.Contains("CaravanWaiting".Translate()))
                __result = __result.Replace("CaravanWaiting".Translate(), s.CapitalizeFirst());
        }
    }
}
