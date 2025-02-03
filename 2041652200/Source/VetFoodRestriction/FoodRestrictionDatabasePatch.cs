using HarmonyLib;
using RimWorld;
using Verse;

namespace VetFoodRestriction
{
	[HarmonyPatch(typeof(FoodRestrictionDatabase), "TryDelete")]
	internal class FoodRestrictionDatabasePatch
	{
		[HarmonyPrefix]
		public static bool PreserveFoodRestriction(FoodRestriction foodRestriction, ref AcceptanceReport __result)
		{
			if (foodRestriction == VFRSaveRestriction.Instance.foodRestiction)
			{
				__result = new AcceptanceReport("VetFoodRestriction : You can't delete this restriction");
				return false;
			}
			return true;
		}
	}
}
