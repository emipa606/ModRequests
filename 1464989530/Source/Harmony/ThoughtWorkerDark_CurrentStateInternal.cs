#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_Dark), "CurrentStateInternal")]
    public static class ThoughtWorkerDark_CurrentStateInternal
    {
        private const int PhotosensDarkThoughtStage = 1;

        public static void Postfix(
            Pawn p,
            ref ThoughtState __result
        )
        {
            if (__result.Active)
            {
                if (p.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
                {
                    switch (comp.PsychDark)
                    {
                        default: return;
                        case VisionType.NVNightVision:
                            __result = ThoughtState.Inactive;

                            return;
                        case VisionType.NVPhotosensitivity:

                            __result = ThoughtState.ActiveAtStage(
                                ThoughtWorkerDark_CurrentStateInternal.PhotosensDarkThoughtStage,
                                VisionType.NVPhotosensitivity.ToString().Translate()
                            );

                            return;
                    }
                }
            }
        }
    }
}