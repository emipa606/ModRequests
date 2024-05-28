using HarmonyLib;
using Verse;

namespace StartWithAnyFluidMeme
{
    [StaticConstructorOnStartup]
    public static class StartWithAnyFluidMeme
    {
        static StartWithAnyFluidMeme()
        {
            var harmony = new Harmony("me.lubar.StartWithAnyFluidMeme");

            harmony.PatchAll();
        }
    }
}
