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
		private static bool cacheIsNPDTiers;
		private static bool hasChachedIsNPDTiers = false;

		private static bool cacheIsReplimat;
		private static bool hasChachedIsReplimat = false;

		private static bool cacheIsRimFridge;
		private static bool hasChachedIsRimFridge = false;

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

		public static bool IsRimFridge()
		{
			if (!hasChachedIsRimFridge)
			{
				cacheIsRimFridge = ModLister.HasActiveModWithName("[KV] RimFridge");
				hasChachedIsRimFridge = true;
			}
			return cacheIsRimFridge;
		}

		public static float MoodEffectFromIngredient(Pawn eater, Thing ingredient)
		{
			var ingestThoughts = new List<ThreadSafeFoodUtility.ThoughtFromIngesting> { };

			bool ateFungus = false;
			bool ateNonFungusRawPlant = false;
			ThreadSafeFoodUtility.AddIngestThoughtsFromIngredient(ingredient.def, eater, ref ingestThoughts, out ateFungus, out ateNonFungusRawPlant);
			if (ModsConfig.IdeologyActive)
			{
				var meatSourceCategory = ingredient.def.IsCorpse ? FoodUtility.GetMeatSourceCategoryFromCorpse(ingredient) : FoodUtility.GetMeatSourceCategory(ingredient.def);
				var AddThoughtsFromIdeoInfo = AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo");
				if (ateNonFungusRawPlant && !ateFungus)
				{
					AddThoughtsFromIdeoInfo.Invoke(null, new object[] { HistoryEventDefOf.AteNonFungusMealWithPlants, eater, ingredient.def, meatSourceCategory });
				}
				if (!FoodUtility.AcceptableVegetarian(ingredient))
				{
					if (!FoodUtility.IsHumanlikeCorpseOrHumanlikeMeatOrIngredient(ingredient) && FoodUtility.GetFoodKind(ingredient) != FoodKind.NonMeat)
					{
						AddThoughtsFromIdeoInfo.Invoke(null, new object[] { HistoryEventDefOf.AteMeat, eater, ingredient.def, meatSourceCategory });
					}
				}
				if (FoodUtility.IsVeneratedAnimalMeatOrCorpse(ingredient.def, eater, ingredient))
				{
					AddThoughtsFromIdeoInfo.Invoke(null, new object[] { HistoryEventDefOf.AteVeneratedAnimalMeat, eater, ingredient.def, meatSourceCategory });
				}
			}

			return ingestThoughts.Sum(x => x.thought.stages[0].baseMoodEffect);
		}
	}
}
