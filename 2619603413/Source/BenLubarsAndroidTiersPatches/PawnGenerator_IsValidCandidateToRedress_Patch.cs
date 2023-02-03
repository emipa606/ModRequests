using HarmonyLib;
using MOARANDROIDS;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(PawnGenerator), "IsValidCandidateToRedress")]
    static class PawnGenerator_IsValidCandidateToRedress_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, PawnGenerationRequest request)
        {
            if (BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value == BoolUnknown.False && request.FixedIdeo != null && pawn.IsBasicAndroidTier())
            {
                __result = false;
            }
        }
    }
}
