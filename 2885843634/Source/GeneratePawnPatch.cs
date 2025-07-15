using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GeneticNaturalFocus
{
    [HarmonyPatch]
    public static class GeneratePawnPatch
    {

        [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
        [HarmonyPostfix]
        public static void Postfix(PawnGenerationRequest request, ref Pawn __result)
        {
            if (__result is null) return;
            if (request.KindDef.RaceProps.Humanlike && MeditationFocusTypeAvailabilityCache.PawnCanUse(__result, MeditationFocusDefOf.Natural))
            {
                //__result.genes ??= new Pawn_GeneTracker(__result);
                if (__result.genes.HasGene(DefOfs.NaturalFocus)) return;
                __result.genes.AddGene(DefOfs.NaturalFocus, false);
            }
        }



    }
}
