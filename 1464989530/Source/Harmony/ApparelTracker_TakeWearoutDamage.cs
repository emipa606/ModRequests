#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "TakeWearoutDamageForDay")]
    public static class ApparelTracker_TakeWearoutDamage
    {
        public static void Postfix(
            Thing ap,
            Pawn_ApparelTracker __instance
        )
        {
            if (ap.Destroyed
                && __instance.pawn is Pawn pawn
                && pawn.RaceProps.Humanlike
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                if (ap is Apparel apparel)
                {
                    comp.RemoveApparel(apparel);
                }
            }
        }
    }
}