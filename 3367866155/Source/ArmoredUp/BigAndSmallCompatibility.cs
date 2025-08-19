using HarmonyLib;
using Verse;

namespace ArmoredUp
{
    [StaticConstructorOnStartup]
    public static class BigAndSmallCompatibility
    {
        public static bool BigAndSmallActive = ModsConfig.IsActive("redmattis.bigsmall.core");

        static BigAndSmallCompatibility()
        {
            if (BigAndSmallActive)
            {
                Log.Message("Armored up: Attempting to patch Big And Small");
                new Harmony("ArmoredUp.BigAndSmallPatch").PatchCategory("Big and Small");
                Log.Message("Armored up: Successfully patched Big And Small!");
            }
        }
    }
}