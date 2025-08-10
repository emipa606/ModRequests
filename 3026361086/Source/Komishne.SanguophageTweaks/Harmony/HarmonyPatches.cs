using HarmonyLib;
using System.Diagnostics;
using Verse;

namespace Komishne.SanguophageTweaks
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("Komishne.SanguophageTweaks");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            harmonyInstance.PatchAll();
            stopwatch.Stop();

            if (SanguophageTweaksSettings.EnableDebugMode)
            {
                Log.Message($"[KOM.SanguophageTweaks] Time for all patches: {stopwatch.Elapsed.TotalMilliseconds}ms.");
            }
        }
    }
}
