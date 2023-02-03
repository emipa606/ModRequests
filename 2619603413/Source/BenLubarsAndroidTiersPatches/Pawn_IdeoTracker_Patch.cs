using HarmonyLib;
using MOARANDROIDS;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(Pawn_IdeoTracker))]
    static class Pawn_IdeoTracker_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.Ideo), MethodType.Getter)]
        public static bool Ideo(ref Ideo __result, Pawn ___pawn)
        {
            if (!___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True)
            {
                return true;
            }

            __result = null;

            if (!FactionIdeosTracker_RecalculateIdeosBasedOnPlayerPawns_Patch.inRecalculate)
            {
                __result = ___pawn.Faction?.ideos?.PrimaryIdeo;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.Certainty), MethodType.Getter)]
        public static bool Certainty(ref float __result, Pawn ___pawn)
        {
            if (!___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True)
            {
                return true;
            }

            __result = 1f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.CertaintyChangePerDay), MethodType.Getter)]
        public static bool CertaintyChangePerDay(ref float __result, Pawn ___pawn)
        {
            if (!___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True)
            {
                return true;
            }

            __result = 0f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.SetIdeo))]
        public static bool SetIdeo(Pawn ___pawn)
        {
            return !___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.IdeoConversionAttempt))]
        public static bool IdeoConversionAttempt(ref bool __result, Pawn ___pawn)
        {
            if (!___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True)
            {
                return true;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Pawn_IdeoTracker.OffsetCertainty))]
        public static bool OffsetCertainty(Pawn ___pawn)
        {
            return !___pawn.IsBasicAndroidTier() || BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.True;
        }
    }
}
