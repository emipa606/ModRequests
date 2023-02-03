using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace BenLubarsAndroidTiersPatches
{
    // oh god why am I doing this
    [HarmonyPatch]
    static class MOARANDROIDS_HediffMaker_Patch_MakeHediff_Patch_Listener_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return Type.GetType("MOARANDROIDS.HediffMaker_Patch,AndroidTiers").GetNestedType("MakeHediff_Patch").GetMethod("Listener");
        }

        public static bool Prefix(ref bool __result)
        {
            __result = true;

            return false;
        }
    }
}
