using HarmonyLib;
using MOARANDROIDS;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(CompAndroidState), "initComp")]
    static class CompAndroidState_initComp_Patch
    {
        public static void Postfix(CompAndroidState __instance)
        {
            var pawn = (Pawn)__instance.parent;
            if (BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value == BoolUnknown.True && pawn.IsBasicAndroidTier())
            {
                pawn.ideo = new Pawn_IdeoTracker(pawn);
            }
        }
    }
}
