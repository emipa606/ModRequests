using HarmonyLib;
using Verse;

namespace RoofedScanning {
    [StaticConstructorOnStartup]
    public static class Entry {
        private static readonly Harmony Harmony;
        static Entry() {
            Entry.Harmony = new Harmony("net.TheElm.RoofedScanning");
            Entry.Harmony.PatchAll();
        }
    }
}
