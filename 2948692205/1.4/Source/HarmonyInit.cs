using HarmonyLib;
using Verse;

namespace IndustrialArmory
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("IndustrialArmory.Mod").PatchAll();
        }
    }
}
