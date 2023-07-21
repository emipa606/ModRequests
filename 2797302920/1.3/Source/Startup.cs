using HarmonyLib;
using Verse;

namespace Renamon
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("Renamon.Mod").PatchAll();
        }
    }
}
