using Verse;

using HarmonyLib;

namespace ImprovedInsectoids
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            if (ModsConfig.IdeologyActive)
            {
                Harmony harmonyInstance = new Harmony("RimWorld.Carnagion.ImprovedInsectoids.HarmonyPatches");
                harmonyInstance.PatchAll();
            }
        }
    }
}
