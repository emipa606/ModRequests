using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    public static class Pawn_TraderTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_TraderTracker))]
        [HarmonyPatch(nameof(Pawn_TraderTracker.CanTradeNow), MethodType.Getter)]
        public static class CanTradeNow
        {
            // Added so guests will always trade, even if they have no goods
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn ___pawn)
            {
                if (!___pawn.IsArrivedGuest(out _)) return true;

                // Copied and removed checks that are also done by IsGuest
                __result = ___pawn.mindState.wantsToTradeWithColony && ___pawn.CanCasuallyInteractNow() && !___pawn.Downed;
                return false;
            }
        }
    }
}
