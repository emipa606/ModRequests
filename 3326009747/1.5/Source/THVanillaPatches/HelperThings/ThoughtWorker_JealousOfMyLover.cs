using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace THVanillaPatches.HelperThings
{
    public class ThoughtWorker_JealousOfMyLover : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!THVanillaPatchesDefsOf.THVP_PolyculeJealousPatch.IsEnabled) return false;
            if (!p.story.traits.HasTrait(TraitDefOf.Jealous) && !p.story.traits.HasTrait(TraitDefOf.Greedy)) return false;
            DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
            if (directPawnRelation == null) return false;
            return !directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() || !directPawnRelation.otherPawn.relations.everSeenByPlayer ? (ThoughtState) false : HasMultiplePartners(directPawnRelation.otherPawn);
        }

        private static bool HasMultiplePartners(Pawn pawnToCheck)
        {
            return LovePartnerRelationUtility.ExistingLovePartners(pawnToCheck).Count > 1;
        }
    }
}