using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace SR
{
    public class PlaceWorker_Soil : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var currentTerrain = loc.GetTerrain(map);
            var underTerrain = map.terrainGrid.UnderTerrainAt(loc);
            var isBridge = currentTerrain.IsBridge(); //It's a bridge..
            if (checkingDef == TerrainDefOf.Ice)
            {
                if (isBridge)
                    return "CannotPlaceIceOnBridge".Translate();
                if (currentTerrain.IsWater || currentTerrain.IsWetBridgeable())
                    return "CannotFillWithIce".Translate();
            }
            if (currentTerrain.driesTo != null && !currentTerrain.affordances.Contains(TerrainAffordanceDefOf.Bridgeable)) //Current is wet non-bridgeable..
                return "CannotPlaceOnWetUnlessBridgeable".Translate();
            if (isBridge && !(underTerrain.IsWater || WaterFreezes_Interop.IsThawableIce(underTerrain))) //If it's a bridge and it isn't water or thawable ice underneath..
                return "CannotPlaceOnBridgeUnlessWaterOrThawableIce".Translate();
            if (currentTerrain == TerrainDefOf.WaterOceanDeep && map.Biome == BiomeDefOf.SeaIce)
                return "CannotFillDeepOceanOnSeaIce".Translate();
            return true;
        }
    }
}