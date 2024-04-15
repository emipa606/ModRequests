using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Runtime.CompilerServices;

namespace SR
{
    public static class TerrainDefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiggable(this TerrainDef def)
        {
            return def.affordances.Contains(TerrainAffordanceDefOf.Diggable) && def.driesTo == null;
        }

        private static Dictionary<TerrainDef, bool> bridgeCache = new Dictionary<TerrainDef, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBridge(this TerrainDef def)
        {
            if (!bridgeCache.ContainsKey(def))
                bridgeCache[def] = def.bridge || def.label.ToLowerInvariant().Contains("bridge") || def.defName.ToLowerInvariant().Contains("bridge");
            return bridgeCache[def];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasSoilPlaceWorker(this TerrainDef def)
        {
            return def.placeWorkers != null && def.placeWorkers.Contains(typeof(PlaceWorker_Soil));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWetBridgeable(this TerrainDef def)
        {
            return def.driesTo != null && def.affordances.Contains(TerrainAffordanceDefOf.Bridgeable);
        }
    }
}