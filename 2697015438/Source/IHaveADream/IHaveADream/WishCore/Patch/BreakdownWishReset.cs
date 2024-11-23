using RimWorld;
using HarmonyLib;
using Verse;
using System;
using Verse.AI;

namespace HDream
{

    [HarmonyPatch(typeof(MentalBreaker), "TryDoRandomMoodCausedMentalBreak")]
    public static class BreakdownWishReset
    {
        public static void Postfix(bool __result, Pawn ___pawn)
        {
            if (!__result || ___pawn.wishes() == null) return;
            ___pawn.wishes().SoftRefresh();
        }
    }
}
