#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
    public static class ApparelTracker_Wear
    {
        public static void Postfix(
            Apparel newApparel,
            Pawn_ApparelTracker __instance
        )
        {
            if (__instance?.pawn is Pawn pawn && pawn.Spawned && pawn.RaceProps.Humanlike)
            {
                if (pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    comp.CheckAndAddApparel(newApparel);
                }
            }
        }
    }
}