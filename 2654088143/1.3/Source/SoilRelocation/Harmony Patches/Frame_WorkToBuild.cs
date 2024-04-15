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
    [HarmonyPatch(typeof(Frame), "WorkToBuild", MethodType.Getter)]
    internal static class Frame_WorkToBuild_get
    {
        internal static void Postfix(Frame __instance, ref float __result)
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