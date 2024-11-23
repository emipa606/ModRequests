using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
    public class WishDef : Def
    {

        public List<string> descriptions;

        public WishTierDef tier;

        public Type wishClass = typeof(Wish);

        public WishCategory category;

        public List<WishCompProperties> comps;

        public ThoughtDef fulfillTought;

        public float upsetPerDay;
        public bool progressAddThought = true;
        public int progressRemovePending = 0;

        public List<WishPreceptFactor> preceptFactor;
        public List<WishTraitFactor> traitFactor;
        public List<WishPassionFactor> passionFactor;
        public List<WishIncapableFactor> incapableFactor;

        public SimpleCurve endChancePerHour;

        protected float baseChance;

        public int minimunAge = -1;

        public int maxCount = 1;
        public float countChanceFactor = 1;

        public bool wantSpecific = false;

        public bool tryPreventSimilare = true;

        public bool countPreWishProgress = true;

        public bool noMoodPreWishProgress = true;

        public float amountNeeded = 1;

        public float progressStep = 1f;

        public string amount_Key = "{Amount}";
        public string covetedObjects_Key = "{Objects}";
        public string listing_Separator = ", ";


        public float naturalMoodBaseKeepChance = 0.33f;
        public float lowNaturalMoodChanceImpact = -0.16f;
        public float highNaturalMoodChanceImpact = 0.40f;

        public bool IsPermanent()
        {
            return endChancePerHour == null;
        }
        protected virtual float GetChance(Pawn pawn, float chance)
        {
            float basedChance = chance;
            float factorChance = 1;
            int count = pawn.wishes().wishes.Where(wish => wish.def == this).Count();
            if (count >= maxCount || pawn.ageTracker.AgeBiologicalYears < minimunAge) return 0;
            for (int i = 0; i < count; i++) factorChance *= countChanceFactor;
            if (ModsConfig.IdeologyActive && pawn.Ideo != null && !preceptFactor.NullOrEmpty())
            {
                for (int i = 0; i < preceptFactor.Count; i++)
                {

                    if (pawn.Ideo.GetPrecept(preceptFactor[i].precept) != null)
                    {
                        if (preceptFactor[i].rebaseChance.HasValue) basedChance = preceptFactor[i].rebaseChance.Value;
                        factorChance *= preceptFactor[i].factor;
                    }
                }
            }
            if (!traitFactor.NullOrEmpty())
            {
                for (int i = 0; i < traitFactor.Count; i++)
                {

                    if (pawn.story.traits.HasTrait(traitFactor[i].trait) && (!traitFactor[i].needDegree || traitFactor[i].degree == pawn.story.traits.DegreeOfTrait(traitFactor[i].trait)))
                    {
                        if (traitFactor[i].rebaseChance.HasValue) basedChance = traitFactor[i].rebaseChance.Value;
                        factorChance *= traitFactor[i].factor;
                    }
                }
            }
            if (!incapableFactor.NullOrEmpty())
            {
                for (int i = 0; i < incapableFactor.Count; i++)
                {
                    if(pawn.WorkTypeIsDisabled(incapableFactor[i].workType))
                        factorChance *= incapableFactor[i].factor;
                }
            }
            if (!passionFactor.NullOrEmpty())
                {
                for (int i = 0; i < passionFactor.Count; i++)
                {
                    switch (pawn.skills.GetSkill(passionFactor[i].skill).passion)
                    {
                        case Passion.Minor:
                            factorChance *= passionFactor[i].minorPassionFactor;
                            break;
                        case Passion.Major:
                            factorChance *= passionFactor[i].majorPassionFactor;
                            break;
                    }
                }
            }

            factorChance *= tier.GetExpectationFactor(ExpectationsUtility.CurrentExpectationFor(pawn));

            return basedChance * factorChance;
        }
        public float GetChance(Pawn pawn)
        {
            return GetChance(pawn, baseChance);
        }
    }
}
