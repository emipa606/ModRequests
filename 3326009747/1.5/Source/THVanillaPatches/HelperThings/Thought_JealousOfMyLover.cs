using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace THVanillaPatches.HelperThings
{
    public class Thought_JealousOfMyLover : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                DirectPawnRelation directPawnRelation = ExistingMostLikedLovePartnerOrRandom(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, false).otherPawn, pawn);
                string label = CurStage.label.Formatted((NamedArgument) directPawnRelation.otherPawn.LabelShort).CapitalizeFirst();
                if (def.Worker != null)
                    label = def.Worker.PostProcessLabel(pawn, label);
                return label;
            }
        }

        protected override float BaseMoodOffset => - 2 - (DoesPawnHaveMoreLovedLover(pawn,
            LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, false).otherPawn, out _) ? 5
            : 0);

        private static DirectPawnRelation ExistingMostLikedLovePartnerOrRandom(Pawn pawn, Pawn otherToExclude)
        {
            if (DoesPawnHaveMoreLovedLover(pawn, otherToExclude, out DirectPawnRelation moreLovedLover))
            {
                return moreLovedLover;
            }
            return LovePartnerRelationUtility.ExistingLovePartners(pawn)
                .Where(relation => relation.otherPawn != otherToExclude).RandomElement();
        }
        
        private static bool DoesPawnHaveMoreLovedLover(Pawn pawnToCheck, Pawn loverToCheck, out DirectPawnRelation moreLovedLover)
        {
            moreLovedLover = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawnToCheck, false);
            return moreLovedLover.otherPawn != loverToCheck;
        }
    }
}