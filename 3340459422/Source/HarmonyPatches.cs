// Modified by llunak, l.lunak@centrum.cz .

using HarmonyLib;
using Verse;

namespace SR.ModRimWorld.RaidExtension;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    static HarmonyPatches()
    {
        var harmonyInstance = new Harmony("com.shadowrabbit.raidextension");
        harmonyInstance.PatchAll();
    }
}
