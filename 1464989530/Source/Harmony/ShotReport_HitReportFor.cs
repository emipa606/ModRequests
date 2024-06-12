#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public static class ShotReport_HitReportFor
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void HitReportFor_Postfix(
            ref ShotReport __result,
            Thing caster,
            Verb verb,
            LocalTargetInfo target
        )
        {


            if (caster is Pawn pawn && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                CurrentShot.Caster = caster;
                CurrentShot.Verb = verb;

                CurrentShot.GlowFactor = CombatHelpers.GlowFactorForPawnAtTarget(pawn, target, comp);

            }
            else
            {
                CurrentShot.ClearLastShot();
            }
        }
    }
}