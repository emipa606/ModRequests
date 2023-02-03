using HarmonyLib;
using MOARANDROIDS;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    static class Pawn_NeedsTracker_ShouldHaveNeed_Patch
    {
        public static bool Postfix(bool __result, Pawn ___pawn, NeedDef nd)
        {
            if (BenLubarsAndroidTiersPatches.Instance.patchNoJoy && nd == NeedDefOf.Joy && ___pawn.IsBasicAndroidTier())
            {
                return false;
            }

            return __result;
        }
    }
}
