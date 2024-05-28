using RimWorld;
using Verse;
using HarmonyLib;

namespace zed_0xff.LoftBed
{
    [StaticConstructorOnStartup]
    public class Init
    {
        static Init()
        {
            Harmony harmony = new Harmony("zed_0xff.LoftBed");
            harmony.PatchAll();
        }
    }
}
