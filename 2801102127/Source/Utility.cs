using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ResumableJobProgress
{
	internal static class Utility
	{
		private static RecipeDef recipeDefOf_MakeStoneBlocksAny = null;
		private static List<RecipeDef> recipeDefResumable = null;

		public static RecipeDef KeyFromRecipe(RecipeDef recipeDef)
		{
			if (recipeDefResumable.NullOrEmpty())
			{
				recipeDefResumable =
				[
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "Make_Patchleather"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "ExtractMetalFromSlag"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "SmeltWeapon"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "SmeltApparel"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "DestroyWeapon"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "DestroyApparel"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "CremateCorpse"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "BurnWeapon"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "BurnApparel"),
					(RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), "BurnDrugs"),
				];
				var recipeDefNames = new List<string>()
				{
					"CookMeal",
					"Make_Pemmican",
					"Make_Kibble",
					"ButcherCorpse",
					"SmashCorpse",
					"Make_ChemfuelFrom"
				};
				foreach (RecipeDef def in GenDefDatabase.GetAllDefsInDatabaseForDef(typeof(RecipeDef)).Cast<RecipeDef>())
				{
					foreach (string recipeDefName in recipeDefNames)
					{
						if (def.defName.Contains(recipeDefName))
						{
							recipeDefResumable.Add(def);
						}
					}
				}
			}

			RecipeDef keyRecipeDef = null;
			if (recipeDefResumable.Contains(recipeDef))
			{
				keyRecipeDef = recipeDef;
			}
			else if (recipeDef.defName.Contains("Make_StoneBlocks"))
			{
				recipeDefOf_MakeStoneBlocksAny ??= (RecipeDef)GenDefDatabase.GetDef(typeof(RecipeDef), BackCompatibility.BackCompatibleDefName(typeof(RecipeDef), "MakeStoneBlocksAny"));
				keyRecipeDef = recipeDefOf_MakeStoneBlocksAny;
			}
			return keyRecipeDef;
		}
	}
}
