using HarmonyLib;
using RimWorld;
using Verse;

namespace GeneticNaturalFocus
{
    [HarmonyPatch]
    public static class MeditationFocusPatch
    {

        [HarmonyPatch(typeof(MeditationFocusTypeAvailabilityCache),
            nameof(MeditationFocusTypeAvailabilityCache.PawnCanUse))]
        [HarmonyPostfix]
        public static void Postfix(Pawn? p, MeditationFocusDef type, ref bool __result)
        {
            if (type == MeditationFocusDefOf.Natural && p is not null && p.genes?.HasGene(DefOfs.NaturalFocus) == true)
            {
                __result = true;
            }
        }

    }
}
