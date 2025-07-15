using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GeneticNaturalFocus
{
    [HarmonyPatch]
    public static class BackCompatibilityPatch
    {


        [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExposeData))]
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
                return;

            if (!GeneticNaturalFocusMod.Settings.AddOnLoad) return;


            if (!__instance.RaceProps.Humanlike ||
                !MeditationFocusTypeAvailabilityCache.PawnCanUse(__instance, MeditationFocusDefOf.Natural)) return;

            try
            {
                if (__instance.genes.HasGene(DefOfs.NaturalFocus)) return;
                {
                    __instance.genes.AddGene(DefOfs.NaturalFocus, false);
                }
            }
            catch
            {
                // ignored
            }
        }

    }
}
