#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using RimWorld;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    public static class VerbMeleeAttack_GetDodgeChance
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void GetDodgeChance_Postfix(
            ref float __result
        )
        {
            if (__result.IsTrivial() || CurrentStrike.GlowDiff.IsTrivial())
            {
                return;
            }

            __result = CombatHelpers.DodgeChanceFunction(__result, CurrentStrike.GlowDiff);
        }
    }
}