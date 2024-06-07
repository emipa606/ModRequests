using RimWorld;
using Verse;
using HarmonyLib;

namespace zed_0xff.GeneCollectorQOL
{
    [StaticConstructorOnStartup]
    public class Init
    {
        static Init()
        {
            Harmony harmony = new Harmony("zed_0xff.GeneCollectorQOL");
            harmony.PatchAll();
            Patch_WVC_GeneSpawner.HandlePatch(harmony);
            Patch_RandomsGeneAssistant.HandlePatch(harmony);
        }
    }
}
