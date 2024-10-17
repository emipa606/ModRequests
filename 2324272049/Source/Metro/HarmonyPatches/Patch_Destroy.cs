using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Metro
{
    [HarmonyPatch(typeof(Mineable), "Destroy")]
    public class Patch_Destroy
    {
        private static void Prefix(Mineable __instance)
        {
            var mapComp = __instance.Map.GetComponent<HiveAIManager>();
            if (mapComp.spidersHiveCells != null && mapComp.spidersHiveCells.Contains(__instance.Position))
            {
                if (mapComp.spidersMinedOutCellsCached == null)
                {
                    mapComp.spidersMinedOutCellsCached = mapComp.GetMinedOutCells(mapComp.spidersHiveCells);
                }
                mapComp.spidersMinedOutCellsCached.Add(__instance.Position);
                mapComp.UpdateSpiderDuties();
            }
            else if (mapComp.nosalisHiveCells != null && mapComp.nosalisHiveCells.Contains(__instance.Position))
            {
                if (mapComp.nosalisesMinedOutCellsCached == null)
                {
                    mapComp.nosalisesMinedOutCellsCached = mapComp.GetMinedOutCells(mapComp.spidersHiveCells);
                }
                mapComp.nosalisesMinedOutCellsCached.Add(__instance.Position);
                mapComp.UpdateNosalisesDuties();
            }
        }
    }
}

