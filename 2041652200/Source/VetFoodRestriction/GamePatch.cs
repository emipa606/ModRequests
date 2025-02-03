using HarmonyLib;
using Verse;

namespace VetFoodRestriction
{
	[HarmonyPatch(typeof(Game), "FinalizeInit")]
	internal class GamePatch
	{
		[HarmonyPostfix]
		public static void AddFoodRestriction(ref Game __instance)
		{
			if (RestrictionUtility.MakeFoodRestriction(__instance.foodRestrictionDatabase))
			{
				Log.Message("VetFoodRestriction : Added the food restriction :" + VFRSaveRestriction.Instance.restrictionName, ignoreStopLoggingLimit: false);
			}
			else
			{
				Log.Message("VetFoodRestriction : The food restriction already exist ! The restrition is named : " + VFRSaveRestriction.Instance.restrictionName, ignoreStopLoggingLimit: false);
			}
		}
	}
}
