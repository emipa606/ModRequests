#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(Hediff), nameof(Hediff.PostAdd))]
    public static class Hediff_PostAdd
    {
        public static void Postfix(
            Hediff __instance
        )
        {
            if (__instance?.pawn is Pawn pawn
                && pawn.Spawned /*&& pawn.RaceProps.Humanlike*/
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision
                            comp)
            {
                comp.CheckAndAddHediff(__instance, __instance.Part);
            }
        }
    }
}