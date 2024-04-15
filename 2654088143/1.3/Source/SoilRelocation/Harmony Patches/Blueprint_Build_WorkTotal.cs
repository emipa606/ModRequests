using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace SR
{
    [HarmonyPatch(typeof(Blueprint_Build), "WorkTotal", MethodType.Getter)]
    internal static class Blueprint_Build_WorkTotal
    {
        internal static void Postfix(Blueprint_Build __instance, ref float __result)
        {
            var map = __instance.Map;
            if (map == null)
                return;
            var newTerrain = __instance.def.entityDefToBuild as TerrainDef;
            if (newTerrain == null) //If it's not a TerrainDef
                return; //We don't need to touch it.
            var cell = __instance.Position;
            var currentTerrain = __instance.Position.GetTerrain(map);
            __result *= HarmonyPatchSharedData.DeriveMultiplierForFill(currentTerrain, newTerrain, false);
        }
    }
}