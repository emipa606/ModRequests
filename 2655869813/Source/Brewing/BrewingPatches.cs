using HarmonyLib;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Brewing
{
    [StaticConstructorOnStartup]
    public static class BrewingPatches
    {
        static BrewingPatches()
        {
            var harmony = new Harmony("me.lubar.VanillaExpandedPatches.Brewing");
            harmony.PatchAll(typeof(BrewingPatches).Assembly);
        }
    }
}
