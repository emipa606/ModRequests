using HarmonyLib;
using VFED;

namespace ImperialFunctionality
{
    [HarmonyPatch(typeof(Building_CrateBiosecured), "GenerateContents")]
    public static class Building_CrateBiosecured_GenerateContents_Patch
    {
        public static bool Prefix(Building_CrateBiosecured __instance)
        {
            if (__instance.questTags?.Contains("MaxpackTribute") ?? false)
            {
                return false;
            }
            return true;
        }
    }


}
