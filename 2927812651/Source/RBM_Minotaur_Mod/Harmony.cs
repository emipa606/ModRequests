using HarmonyLib;
using Verse;

namespace RBM_Minotaur
{
    [StaticConstructorOnStartup]
    // Sets up Harmony Patches
    public static class RBM_Minotaur
    {
        static RBM_Minotaur()
        {
            Harmony harmony = new Harmony("rimworld.mod.rooboid.minotaur");
            harmony.PatchAll();
            Log.Message("RBM_Minotaur Mod Harmony Patches Loaded");
        }
    }
}
