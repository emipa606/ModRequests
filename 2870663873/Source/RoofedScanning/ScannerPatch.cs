using HarmonyLib;
using RimWorld;

namespace RoofedScanning {
    [HarmonyPatch(typeof(CompScanner))]
    [HarmonyPatch("CanUseNow", MethodType.Getter)]
    public static class ScannerPatch {
        public static bool Prefix(
            CompScanner __instance,
            CompPowerTrader ___powerComp,
            CompForbiddable ___forbiddable,
            ref bool __result
        ) {
            __result = __instance.parent.Spawned && (___powerComp == null || ___powerComp.PowerOn) && (___forbiddable == null || !___forbiddable.Forbidden) && __instance.parent.Faction == Faction.OfPlayer;
            return false; // Don't ever fallback
        }
    }
}
