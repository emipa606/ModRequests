using HarmonyLib;
using Verse;

namespace BlueprintMaterialDebt.ToggleableReadout
{
    [StaticConstructorOnStartup]
    static class ToggleableReadoutPatches
    {
        static ToggleableReadoutPatches()
        {
            var harmony = new Harmony("me.lubar.BlueprintMaterialDebt.ToggleableReadout");
            harmony.PatchAll();
        }
    }
}
