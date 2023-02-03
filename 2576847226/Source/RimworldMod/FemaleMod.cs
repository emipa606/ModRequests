using System;
using HugsLib;
using RimWorld;
using HarmonyLib;
using Verse;
using Verse.AI.Group;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse.Sound;
using Verse.AI;
using System.Linq;

namespace RimworldMod
{
	[StaticConstructorOnStartup]
	public class FemaleMod
	{
		static FemaleMod()
		{
			var harmony = new Harmony("kentington.futureisfemale");
			harmony.PatchAll();
		}

		public static bool RemoveUnwantedBehaviorFromExistingSaves(Pawn a, Pawn b)
		{
			return a.gender!=Gender.Female && b.gender!=Gender.Female;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator),"GenerateOrRedressPawnInternal")]
	public static class AllFemale
	{
		[HarmonyPrefix]
		public static void MakeOnlyLadies(ref PawnGenerationRequest request)
		{
			if(request.KindDef.RaceProps.hasGenders && request.KindDef.RaceProps.Humanlike)
				request = new PawnGenerationRequest(request.KindDef, request.Faction, request.Context, request.Tile, request.ForceGenerateNewPawn, request.Newborn, request.AllowDead, request.AllowDowned, request.CanGeneratePawnRelations, request.MustBeCapableOfViolence, request.ColonistRelationChanceFactor, request.ForceAddFreeWarmLayerIfNeeded, request.AllowGay, request.AllowFood, request.AllowAddictions, request.Inhabitant, request.CertainlyBeenInCryptosleep, request.ForceRedressWorldPawnIfFormerColonist, request.WorldPawnFactionDoesntMatter, request.BiocodeWeaponChance, request.BiocodeApparelChance, request.ExtraPawnForExtraRelationChance, request.RelationWithExtraPawnChanceFactor, request.ValidatorPreGear, request.ValidatorPostGear, request.ForcedTraits, request.ProhibitedTraits, request.MinChanceToRedressWorldPawn, request.FixedBiologicalAge, request.FixedChronologicalAge, Gender.Female, request.FixedMelanin, request.FixedLastName);
		}
	}

	[HarmonyPatch(typeof(PawnRelationWorker_Sibling),"CreateRelation")]
	public static class FatherFigure
	{
		[HarmonyPrefix]
		public static bool stopExecution()
		{
			return false;
		}

		public static void sheHasDaddyIssues(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			bool flag = other.GetMother() != null;
			if (!flag)
			{
				Pawn newMother = (Pawn)typeof(PawnRelationWorker_Sibling).GetMethod ("GenerateParent", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,new object[5] {generated, other, Gender.Female, request, false});
				other.SetMother(newMother);
			}
			generated.SetMother(other.GetMother());
			generated.SetFather(other.GetFather());
			typeof(PawnRelationWorker_Sibling).GetMethod ("ResolveMyName", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,new object[2] {request, generated});
			typeof(PawnRelationWorker_Sibling).GetMethod ("ResolveMySkinColor", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,new object[2] {request, generated});
		}
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "IsBioUseable")]
	public static class RemoveIncompatibleBios
	{
		[HarmonyPostfix]
		public static void Nope(PawnBio bio, ref bool __result)
		{
			if (bio != null && bio.childhood != null && (bio.childhood.identifier == "CoreworldStudent50" || bio.childhood.identifier == "CaravanChild53"))
				__result = false;
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt),"RandomSelectionWeight")]
	public static class GirlOnGirlAction
	{
		[HarmonyPrefix]
		public static bool DisableOriginalMethod()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Replacement(Pawn initiator, Pawn recipient, ref float __result)
		{
			if (FemaleMod.RemoveUnwantedBehaviorFromExistingSaves (initiator, recipient)) {
				__result = 0f;
				return;
			}
			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				__result = 0f;
				return;
			}
			float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (num < 0.25f)
			{
				__result = 0f;
				return;
			}
			int num2 = initiator.relations.OpinionOf(recipient);
			if (num2 < 5)
			{
				__result = 0f;
				return;
			}
			if (recipient.relations.OpinionOf(initiator) < 5)
			{
				__result = 0f;
				return;
			}
			float num3 = 1f;
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
			if (pawn != null)
			{
				float value = (float)initiator.relations.OpinionOf(pawn);
				num3 = Mathf.InverseLerp(50f, -50f, value);
			}
			float num4 = 1f;
			float num5 = Mathf.InverseLerp(0.25f, 1f, num);
			float num6 = Mathf.InverseLerp(5f, 100f, (float)num2);
			float num7 = 1f;
			__result= 1.15f * num4 * num5 * num6 * num3 * num7;
		}
	}

	[HarmonyPatch(typeof(LovePartnerRelationUtility),"LovePartnerRelationGenerationChance")]
	public static class MoreGirlOnGirlAction
	{
		[HarmonyPrefix]
		public static bool DisableOriginalMethod()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Replacement(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex, ref float __result)
		{
			if (FemaleMod.RemoveUnwantedBehaviorFromExistingSaves (generated, other)) {
				__result = 0f;
				return;
			}
			if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				__result = 0f;
				return;
			}
			if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
			{
				__result = 0f;
				return;
			}
			float num = 1f;
			if (ex)
			{
				int num2 = 0;
				List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
				for (int i = 0; i < directRelations.Count; i++)
				{
					if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
					{
						num2++;
					}
				}
				num = Mathf.Pow(0.2f, (float)num2);
			}
			else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
			{
				__result = 0f;
				return;
			}
			float num3 = 1f;
			float generationChanceAgeFactor = (float)typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeFactor",BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[1] {generated});
			float generationChanceAgeFactor2 = (float)typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeFactor",BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[1] {other});
			float generationChanceAgeGapFactor = (float)typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeGapFactor",BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[3] {generated, other, ex});
			float num4 = 1f;
			if(generated.relations.FamilyByBlood.Contains(other))
			{
				num4 = 0.01f;
			}
			float num5;
			if (request.FixedMelanin.HasValue)
			{
				num5 = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
			}
			else
			{
				num5 = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
			}
			__result = num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
		}
	}

	[HarmonyPatch(typeof(Pawn_RelationsTracker),"SecondaryLovinChanceFactor")]
	public static class EvenMoreGirlOnGirlAction
	{
		[HarmonyPrefix]
		public static bool DisableOriginalMethod()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Replacement(Pawn otherPawn, Pawn_RelationsTracker __instance, ref float __result)
		{
			Pawn pawn = (Pawn)typeof(Pawn_RelationsTracker).GetField ("pawn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
			if (FemaleMod.RemoveUnwantedBehaviorFromExistingSaves (pawn, otherPawn)) {
				__result = 0f;
				return;
			}
			if (pawn == otherPawn || pawn.def.race.intelligence!=Intelligence.Humanlike || otherPawn.def.race.intelligence!=Intelligence.Humanlike)
			{
				__result = 0f;
				return;
			}
			float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
			float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
			float num = 1f;
				if (ageBiologicalYearsFloat2 < 16f)
				{
					__result = 0f;
					return;
				}
				if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f)
				{
					__result = 0.15f;
					return;
				}
				if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
				{
					num = Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.3f;
				}
				else
				{
					num = GenMath.FlatHill(0.3f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.15f, ageBiologicalYearsFloat2);
				}
			float num2 = 1f;
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
			num2 *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
			int num3 = 0;
			if (otherPawn.RaceProps.Humanlike)
			{
				num3 = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
			}
			float num4 = 1f;
			if (num3 < 0)
			{
				num4 = 0.3f;
			}
			else if (num3 > 0)
			{
				num4 = 2.3f;
			}
			float num5 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
			float num6 = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
			__result= num * num2 * num5 * num6 * num4;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator),"GenerateTraits")]
	public static class YetMoreGirlOnGirlAction
	{
		[HarmonyPrefix]
		public static bool DisableOriginalMethod()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void Replacement(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.story == null)
			{
				return;
			}
			if (pawn.story.childhood.forcedTraits != null)
			{
				List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
				for (int i = 0; i < forcedTraits.Count; i++)
				{
					TraitEntry traitEntry = forcedTraits[i];
					if (traitEntry.def == null)
					{
						Log.Error("Null forced trait def on " + pawn.story.childhood, false);
					}
					else if (!pawn.story.traits.HasTrait(traitEntry.def) && traitEntry.def != TraitDefOf.Gay)
					{
						pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree, false));
					}
				}
			}
			if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null)
			{
				List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
				for (int j = 0; j < forcedTraits2.Count; j++)
				{
					TraitEntry traitEntry2 = forcedTraits2[j];
					if (traitEntry2.def == null)
					{
						Log.Error("Null forced trait def on " + pawn.story.adulthood, false);
					}
					else if (!pawn.story.traits.HasTrait(traitEntry2.def) && traitEntry2.def != TraitDefOf.Gay)
					{
						pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree, false));
					}
				}
			}
			int num = Rand.RangeInclusive(2, 3);
            Predicate<SkillDef> SkillConflict=null;
            while (pawn.story.traits.allTraits.Count < num)
			{
				TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
				if (!pawn.story.traits.HasTrait(newTraitDef))
				{
					if (newTraitDef == TraitDefOf.Gay || newTraitDef == TraitDefOf.Bisexual || newTraitDef == TraitDefOf.Asexual || newTraitDef==TraitDefOf.DislikesMen || newTraitDef ==TraitDefOf.DislikesWomen)
					{
						continue;
					}
					if (request.Faction == null || Faction.OfPlayerSilentFail == null || !request.Faction.HostileTo(Faction.OfPlayer) || newTraitDef.allowOnHostileSpawn)
					{
						if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
						{
							if (newTraitDef.requiredWorkTypes == null || (!pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes) && !pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags)))
							{
                                if (newTraitDef.forcedPassions != null && pawn.workSettings != null)
                                {
                                    List<SkillDef> arg_4DB_0 = newTraitDef.forcedPassions;
                                    Predicate<SkillDef> arg_4DB_1;
                                    if ((arg_4DB_1 = SkillConflict) == null)
                                    {
                                        arg_4DB_1 = (SkillConflict = ((SkillDef p) => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(true))));
                                    }
                                    if (arg_4DB_0.Any(arg_4DB_1))
                                    {
                                        continue;
                                    }
                                }
                                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
									if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
									{
										Trait trait2 = new Trait(newTraitDef, degree, false);
										if (pawn.mindState != null && pawn.mindState.mentalBreaker != null)
										{
											float num2 = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
											num2 += trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold);
											num2 *= trait2.MultiplierOfStat(StatDefOf.MentalBreakThreshold);
											if (num2 > 0.4f)
											{
												continue;
											}
										}
										pawn.story.traits.GainTrait(trait2);
									}
							}
						}
					}
				}
			}
		}
	}

    [HarmonyPatch(typeof(GameCondition_PsychicEmanation), "PostMake")]
    public static class PsychicPatch
    {
        [HarmonyPostfix]
        public static void AllFemale(GameCondition_PsychicEmanation __instance)
        {
            __instance.gender = Gender.Female;
        }
    }

    [HarmonyPatch(typeof(GameCondition_PsychicSuppression), "Init")]
    public static class PsychicPatch2
    {
        [HarmonyPostfix]
        public static void AllFemale(GameCondition_PsychicSuppression __instance)
        {
            __instance.gender = Gender.Female;
        }
    }

	[HarmonyPatch(typeof(CompAbilityEffect_WordOfLove), "ValidateTarget")]
	public static class PsycastPatch1
    {
		[HarmonyPrefix]
		public static bool PreventExecution()
        {
			return false;
        }

		[HarmonyPostfix]
		public static void NoTest(LocalTargetInfo target, bool showMessages, ref bool __result, CompAbilityEffect_WordOfLove __instance)
        {
			Pawn pawn = (Pawn)__instance.Caster;
			Pawn pawn2 = target.Pawn;
			if (pawn == pawn2)
			{
				__result = false;
			}
			else
            {
				__result = true;
            }
		}
    }
}