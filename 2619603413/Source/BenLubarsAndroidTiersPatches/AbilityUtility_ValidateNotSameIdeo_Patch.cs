using System;
using HarmonyLib;
using MOARANDROIDS;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(AbilityUtility), nameof(AbilityUtility.ValidateNotSameIdeo), new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool) })]
    static class AbilityUtility_ValidateNotSameIdeo_Patch
    {
        public static void Postfix(ref bool __result, Pawn casterPawn, Pawn targetPawn, bool showMessages)
        {
            if (BenLubarsAndroidTiersPatches.Instance.patchBasicIdeoligion.Value != BoolUnknown.Unknown && __result && targetPawn.IsBasicAndroidTier())
            {
                if (showMessages)
                {
                    Messages.Message("BenLubarsAndroidTiersPatches_AbilityCantHaveIdeo".Translate(targetPawn, casterPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
                }

                __result = false;
            }
        }
    }
}
