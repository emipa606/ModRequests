using RimWorld;
using Verse;

namespace VetFoodRestriction
{
	internal class RestrictionUtility
	{
		public static bool MakeFoodRestriction(FoodRestrictionDatabase foodRestrictionDatabase)
		{
			foreach (FoodRestriction allFoodRestriction in foodRestrictionDatabase.AllFoodRestrictions)
			{
				if (allFoodRestriction.label == VFRSaveRestriction.Instance.restrictionName && allFoodRestriction.id == VFRSaveRestriction.Instance.restrictionId)
				{
					if (VFRSaveRestriction.Instance.foodRestiction != allFoodRestriction)
					{
						VFRSaveRestriction.Instance.foodRestiction = allFoodRestriction;
					}
					return false;
				}
			}
			FoodRestriction foodRestriction = foodRestrictionDatabase.MakeNewFoodRestriction();
			foodRestriction.label = "VetFoodRestriction";
			VFRSaveRestriction.Instance.foodRestiction = foodRestriction;
			VFRSaveRestriction.Instance.restrictionName = foodRestriction.label;
			VFRSaveRestriction.Instance.restrictionId = foodRestriction.id;
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.ingestible != null && (int)allDef.ingestible.preferability >= 7 && allDef != ThingDefOf.MealNutrientPaste && allDef != ThingDefOf.Pemmican)
				{
					foodRestriction.filter.SetAllow(allDef, allow: false);
				}
			}
			foodRestriction.filter.SetAllow(ThingDefOf.Chocolate, allow: false);
			return true;
		}
	}
}
