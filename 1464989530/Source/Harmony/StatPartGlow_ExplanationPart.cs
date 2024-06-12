#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(StatPart_Glow), nameof(StatPart_Glow.ExplanationPart))]
    public static class StatPartGlow_ExplanationPart
    {
        public static void Postfix(
            ref StatRequest req,
            ref string __result
        )
        {
            if (!__result.NullOrEmpty()
                && req.Thing is Pawn pawn
                && pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp)
            {
                float glowat = pawn.Map.glowGrid.GameGlowAt(pawn.Position);

                if (glowat < 0.3f || glowat > 0.7f)
                {
                    __result = "StatsReport_LightMultiplier".Translate(glowat.ToStringPercent())
                               + ": ";

                    __result += StatReportFor_NightVision.ShortStatReport(glowat, comp);
                }
            }
        }
    }
}