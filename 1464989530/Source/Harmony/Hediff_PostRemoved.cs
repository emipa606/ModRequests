#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using Verse;

namespace NightVision.Harmony
{

    //HediffWithComps - because this class, derived from Hediff, doesn't use base.PostAdd

    [HarmonyPatch(typeof(Hediff), nameof(Hediff.PostRemoved))]
    public static class Hediff_PostRemoved
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
                comp.RemoveHediff(__instance, __instance.Part);
            }
        }
    }
}