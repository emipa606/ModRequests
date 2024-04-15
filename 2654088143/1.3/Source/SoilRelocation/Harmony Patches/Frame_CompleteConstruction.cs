using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace SR
{
    //This is a harmony patch but it is performed conditionally and thus does not have the HarmonyPatch attribute.
    internal static class Frame_CompleteConstruction
    {
        internal static void Prefix(Frame __instance)
        {
            var map = __instance.Map;
            if (map == null)
                return; 
            var cell = __instance.Position;
            var oldTerrain = cell.GetTerrain(map);
            //If it was water and it has become soil (but not ice).
            if (oldTerrain.IsWater && __instance.def.entityDefToBuild is TerrainDef tDef && tDef.IsDiggable() && tDef.defName != "Ice")
                WaterFreezes_Interop.ClearCellNaturalWater(map, cell); //Water depth and AllWater values are set properly by the SetTerrain patch in WF during the actual method after this.
        }
    }
}