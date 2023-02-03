using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BenLubarsAndroidTiersPatches
{
    [HarmonyPatch(typeof(HediffGiverUtility), nameof(HediffGiverUtility.TryApply), new Type[] { typeof(Pawn), typeof(HediffDef), typeof(IEnumerable<BodyPartDef>), typeof(bool), typeof(int), typeof(List<Hediff>) })]
    static class HediffGiverUtility_TryApply_Patch
    {
        static bool Prefix(ref bool __result, ref Pawn pawn, ref HediffDef hediff)
        {
            if (BenLubarsAndroidTiersPatches.Instance.patchChronicDiseases && pawn.IsAndroid() && hediff.chronic)
            {
                __result = false;

                return false;
            }

            return true;
        }
    }
}
