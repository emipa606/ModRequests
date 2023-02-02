using HarmonyLib;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Fishing
{
    [StaticConstructorOnStartup]
    public static class FishingPatches
    {
        static FishingPatches()
        {
            var harmony = new Harmony("me.lubar.VanillaExpandedPatches.Fishing");
            harmony.PatchAll(typeof(FishingPatches).Assembly);
        }
    }
}
