using HarmonyLib;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Insectoids
{
    [StaticConstructorOnStartup]
    public static class InsectoidsPatches
    {
        static InsectoidsPatches()
        {
            var harmony = new Harmony("me.lubar.VanillaExpandedPatches.Insectoids");
            harmony.PatchAll(typeof(InsectoidsPatches).Assembly);
        }
    }
}
