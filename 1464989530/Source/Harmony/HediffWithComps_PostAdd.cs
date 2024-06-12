#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(HediffWithComps), nameof(HediffWithComps.PostAdd))]
    public static class HediffWithComps_PostAdd
    {
        public static void Postfix(
            HediffWithComps __instance
        )
        {
            if (__instance.pawn is Pawn pawn
                && pawn.Spawned
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp
            )
            {
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
    }
}