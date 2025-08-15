using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_PawnRenderFlagsExtension
    {
        public static bool FlagSet_Prefix(bool __result, PawnRenderFlags flag)
        {
            if (StylishModSettings.commonizationPortrait && flag == PawnRenderFlags.Portrait)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
