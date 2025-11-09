using HarmonyLib;
using RimWorld;
using VariedBodySizes;
using Verse;

// ReSharper disable InconsistentNaming

namespace genebodysize;

public class HarmonyPatches
{
    [HarmonyPatch(typeof(VariedBodySizes_GameComponent), "OnCalculateBodySize")]
    public static class Pawn_BodySizePatch_OnCalculateBodySizePatch
    {
        private static readonly TimedCache<float> statCache = new(36);
        private static StatDef bodySizeMultiplier;
        // Register a modifier for ez access
        public static void Postfix(ref float __result, Pawn pawn)
        {
            // GetStatValue will access needs, which if null, will break loading /tableflip
            if (!ModsConfig.BiotechActive || pawn?.RaceProps.Humanlike != true || pawn.needs == null && !pawn.Dead)
                return;

            // cached value, or calculate/cache/return
            if (statCache.TryGet(pawn, out var pawnSize) && pawnSize > 0.01f) {
                __result = pawnSize;
                return;
            }
        
            // Load-time fun!
            bodySizeMultiplier ??= StatDef.Named("bodySizeFactor");
            var bodySizeMultiplierValue = pawn.GetStatValue(bodySizeMultiplier, cacheStaleAfterTicks: 360);
            statCache.Set(pawn, __result *= bodySizeMultiplierValue);
        }
        
    }
}