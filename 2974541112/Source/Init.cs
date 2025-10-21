using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

[StaticConstructorOnStartup]
public class Init
{
    static Init()
    {
        Harmony harmony = new Harmony("zed_0xff.CPS");
        harmony.PatchAll();
    }
}
