using HarmonyLib;
using VFED;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(Building_SupplyCrate), "GenerateContents")]
    public static class Building_SupplyCrate_GenerateContents_Patch
    {
        public static bool Prefix(Building_SupplyCrate __instance)
        {
            if (__instance.questTags?.Contains("MaxpackTribute") ?? false)
            {
                return false;
            }
            return true;
        }
    }
}
