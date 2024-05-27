using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.VNPE_Fridge_Fix
{
    [StaticConstructorOnStartup]
    public class Init
    {
        static Init()
        {
            Harmony harmony = new Harmony("zed_0xff.VNPE_Fridge_Fix");
            harmony.PatchAll();
        }
    }
}
