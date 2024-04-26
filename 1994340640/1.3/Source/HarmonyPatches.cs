using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace WallStuff
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.arcjc.wallstuff");

            var originalLaunchThingsOfTypeMethod = typeof(TradeUtility).GetMethod("LaunchThingsOfType");
            var patchedLaunchThingsOfTypeMethod = typeof(WallMountedTradeUtility).GetMethod("LaunchThingsOfType");

            var originalAllLaunchableThingsForTradeMethod = typeof(TradeUtility).GetMethod("AllLaunchableThingsForTrade");
            var patchedAllLaunchableThingsForTradeMethod = typeof(WallMountedTradeUtility).GetMethod("AllLaunchableThingsForTrade");

            var originalAllPoweredMethod = typeof(Building_OrbitalTradeBeacon).GetMethod("AllPowered");
            var patchedAllPoweredMethod = typeof(WallTradeBeacon).GetMethod("AllPoweredHarmonyPatch");

            harmony.Patch(originalLaunchThingsOfTypeMethod, postfix: new HarmonyMethod(patchedLaunchThingsOfTypeMethod));
            harmony.Patch(originalAllLaunchableThingsForTradeMethod, postfix: new HarmonyMethod(patchedAllLaunchableThingsForTradeMethod));
            harmony.Patch(originalAllPoweredMethod, postfix: new HarmonyMethod(patchedAllPoweredMethod));
            harmony.PatchAll();
        }

    }
}