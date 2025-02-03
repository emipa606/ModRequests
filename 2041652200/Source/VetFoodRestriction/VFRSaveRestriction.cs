using HarmonyLib;
using RimWorld;
using Verse;

namespace VetFoodRestriction
{
	[HarmonyPatch(typeof(FoodRestrictionDatabase), "ExposeData")]
	internal class VFRSaveRestriction
	{
		public string restrictionName;

		public int restrictionId;

		public FoodRestriction foodRestiction;

		private static VFRSaveRestriction instance;

		public static VFRSaveRestriction Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VFRSaveRestriction();
				}
				return instance;
			}
		}

		public static void Prefix()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Instance.restrictionName = Instance.foodRestiction.label;
				Instance.restrictionId = Instance.foodRestiction.id;
			}
			Scribe_Values.Look(ref Instance.restrictionName, "Sielfyr.VetFoodRestrictionrestrictionName", "VetFoodRestriction");
			Scribe_Values.Look(ref Instance.restrictionId, "Sielfyr.VetFoodRestrictionrestrictionId", -1);
		}
	}
}
