using HarmonyLib;
using System.Diagnostics;
using Verse;

namespace Komishne.SanguophageTweaks.Conditional.VRES
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("Komishne.SanguophageTweaks.Conditional.VRES");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            harmonyInstance.PatchAll();
            stopwatch.Stop();

            if (SanguophageTweaksSettings.EnableDebugMode)
            {
                Log.Message($"[KOM.SanguophageTweaks.Conditional.VRES] Time for all patches: {stopwatch.Elapsed.TotalMilliseconds}ms.");
            }
        }
    }
}
