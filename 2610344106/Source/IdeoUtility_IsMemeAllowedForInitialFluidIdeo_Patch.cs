using System;
using HarmonyLib;
using RimWorld;

namespace StartWithAnyFluidMeme
{
    [HarmonyPatch(typeof(IdeoUtility), nameof(IdeoUtility.IsMemeAllowedForInitialFluidIdeo), new Type[] { typeof(MemeDef) })]
    static class IdeoUtility_IsMemeAllowedForInitialFluidIdeo_Patch
    {
        static bool Prefix(ref bool __result, ref MemeDef memeDef)
        {
            __result = true;
            return false;
        }
    }
}
