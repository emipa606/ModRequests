using HarmonyLib;
using Verse;

namespace WallStuff.Paste
{    
		[HarmonyPatch(typeof(ThingListGroupHelper), "Includes")]
		public static class ThingListGroupHelper_Includes
		{
			public static void Postfix(ThingDef def, ThingRequestGroup group, ref bool __result)
			{
				if ((group == ThingRequestGroup.FoodSourceNotPlantOrTree || group == ThingRequestGroup.FoodSource) && def.defName == "WallMountedNutrientPasteTap")
				{
					__result = true;
				}
			}
		}
}
