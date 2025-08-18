using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

// NPDTiers patch
using NutrientPasteTiers;

namespace SaferPasteDispenser
{
	public static class SaferPasteUtility
	{
		public struct ThoughtFromIngesting
		{
			public ThoughtDef thought;

			public Precept fromPrecept;
		}

		private static bool cacheIsNPDTiers;
		private static bool hasChachedIsNPDTiers = false;

		private static bool cacheIsReplimat;
		private static bool hasChachedIsReplimat = false;

		private static bool cacheIsVNPE;
		private static bool hasChachedIsVNPE = false;

		public static bool IsNPDTiers()
		{
			if (!hasChachedIsNPDTiers)
			{
				cacheIsNPDTiers = ModLister.HasActiveModWithName("NPDTiers - The Nutrient Paste Expansion Mod (Continued)");
				hasChachedIsNPDTiers = true;
			}
			return cacheIsNPDTiers;
		}

		public static bool IsNPDTiers(Building_NutrientPasteDispenser dispenser)
		{
			return dispenser.def.GetModExtension<NutrientPasteCustom>()?.customMeal != ThingDefOf.MealNutrientPaste;
		}

		public static bool IsReplimat()
		{
			if (!hasChachedIsReplimat)
			{
				cacheIsReplimat = ModLister.HasActiveModWithName("Replimat");
				hasChachedIsReplimat = true;
			}
			return cacheIsReplimat;
		}

		public static bool IsVNPE()
		{
			if (!hasChachedIsVNPE)
			{
				cacheIsVNPE = ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded");
				hasChachedIsVNPE = true;
			}
			return cacheIsVNPE;
		}

		public static bool IsIncompatibleMod()
		{
			return IsReplimat() || IsVNPE();
		}

		public static List<ThoughtFromIngesting> ThoughtsFromIngesting(Pawn ingester, Thing ingredient)
		{
			List<ThoughtFromIngesting> ingestThoughts = [];
			if (ingester.needs is null || ingester.needs.mood is null)
			{
				return ingestThoughts;
			}
			ThingDef ingredientDef = ingredient.def;
			MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(ThingDefOf.MealNutrientPaste);
			{
				ThreadSafeFoodUtility.AddIngestThoughtsFromIngredient(ingredientDef, ingester, ref ingestThoughts, out var ateFungus, out var ateNonFungusRawPlant);
				if (ModsConfig.IdeologyActive && ateNonFungusRawPlant && !ateFungus)
				{
					ThreadSafeFoodUtility.AddThoughtsFromIdeo(HistoryEventDefOf.AteNonFungusMealWithPlants, ingester, ref ingestThoughts, ThingDefOf.MealNutrientPaste, meatSourceCategory);
				}
			}
			return ingestThoughts;
		}

		public static float MoodEffectFromIngredient(Pawn ingester, Thing ingredient)
		{
			List<ThoughtFromIngesting> ingestThoughts = ThoughtsFromIngesting(ingester, ingredient);
			return ingestThoughts.Sum(x => x.thought.stages[0].baseMoodEffect);
		}
	}
}
