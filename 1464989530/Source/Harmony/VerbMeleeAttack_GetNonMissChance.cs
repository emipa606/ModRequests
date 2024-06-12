#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetNonMissChance")]
    public static class VerbMeleeAttack_GetNonMissChance
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void GetNonMissChance_Postfix(ref Verb __instance, ref float __result, LocalTargetInfo target)
        {
            if (__result > 0.999f || !(__instance.CasterPawn is Pawn pawn) || !(pawn.TryGetComp<Comp_NightVision>() is Comp_NightVision comp))
            {
                CurrentStrike.GlowFactor = Constants.TRIVIAL_FACTOR;
                CurrentStrike.GlowDiff = 0f;

                return;
            }

            float glow = pawn.Map != null ? GlowFor.GlowAt(pawn.Map, target.Cell) : Constants.TRIVIAL_GLOW;

            if (!glow.GlowIsDarkOrBright())
            {
                CurrentStrike.GlowFactor = Constants.TRIVIAL_FACTOR;
                CurrentStrike.GlowDiff = 0f;

                return;
            }

            CurrentStrike.GlowFactor = comp.FactorFromGlow(glow);

            if (target.Thing is Pawn t_pawn)
            {
                CurrentStrike.GlowDiff = CurrentStrike.GlowFactor - GlowFor.FactorOrFallBack(t_pawn, glow);

                if (CurrentStrike.GlowDiff.IsNonTrivial() && CurrentStrike.SurpAtkSuccess)
                {
                    AccessTools.FieldRefAccess<Verb, bool>(__instance, "surpriseAttack") = true;
                    __result = 1f;
                    MoteMaker.ThrowText(pawn.Position.ToVector3Shifted(), pawn.Map, "NightVisionSneakAtkMote".Translate(), 5f);

                    return;
                }
            }
            else
            {
                CurrentStrike.GlowDiff = 0;
            }

            __result = CombatHelpers.HitChanceGlowTransform(__result, CurrentStrike.GlowFactor);
        }
    }
}