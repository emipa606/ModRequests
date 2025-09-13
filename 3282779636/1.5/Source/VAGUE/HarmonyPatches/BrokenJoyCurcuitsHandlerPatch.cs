using RimWorld;
using Verse;
using HarmonyLib;
using VREAndroids;

namespace VAGUE
{
    [HarmonyPatch(typeof(Gene_SyntheticBody), "Tick")]
    internal class BrokenJoyCurcuitsHandlerPatch
    {
        [HarmonyPrefix]
        public static bool EnsureBadBreakForPsychopathAndroids(Gene_SyntheticBody __instance)
        {
            if (__instance.pawn.needs.mood.CurLevel > 0.05f && __instance.pawn.needs.mood.CurLevel < 0.8f)
            {
                return true;
            }

            if (!__instance.pawn.IsHashIntervalTick(2500) || !__instance.pawn.HasActiveGene(VAGUE_InternalDefs.VAGUE_largeMoodDecrease) || __instance.pawn.IsAwakened())
            {
                return true;
            }

            __instance.Awaken("VREA.AndroidAwakening".Translate(__instance.pawn.Named("PAWN")), "VREA.AndroidAwakeningLowMood".Translate(__instance.pawn.Named("PAWN")));
            var gene = __instance.pawn.genes.GetGene(VREA_DefOf.VREA_CombatIncapability);
            if (gene != null)
            {
                __instance.pawn.genes.RemoveGene(gene);
            }
            __instance.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            __instance.pawn.story.traits.GainTrait(new Trait(TraitDefOf.Psychopath, forced: true) , true);
            return false;
        }
    }
}
