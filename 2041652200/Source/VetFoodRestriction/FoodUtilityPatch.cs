using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VetFoodRestriction
{
	internal class FoodUtilityPatch
	{
		public static bool WillFeedPatientAnimal(bool willEatValue, Pawn p, Pawn getter)
		{
			return willEatValue && p.RaceProps.Animal && getter.IsColonist && FeedPatientUtility.ShouldBeFed(p);
		}
	}
	
	// "old"  WillEat
	[HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[]
	{
		typeof(Pawn),
		typeof(Thing),
		typeof(Pawn),
		typeof(bool)
	})]
	internal class FoodUtilityPatch1
	{
		[HarmonyPriority(200)]
		[HarmonyPostfix]
		public static void WillEatPatch1(ref bool __result, Pawn p, Thing food, Pawn getter)
		{
			if (getter != null && FoodUtilityPatch.WillFeedPatientAnimal(__result, p, getter))
			{
				__result = VFRSaveRestriction.Instance.foodRestiction.Allows(food);
			}
		}
	}
	[HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[]
	{
		typeof(Pawn),
		typeof(ThingDef),
		typeof(Pawn),
		typeof(bool)
	})]
	internal class FoodUtilityPatch2
	{
		[HarmonyPriority(200)]
		[HarmonyPostfix]
		public static void WillEatPatch2(ref bool __result, Pawn p, ThingDef food, Pawn getter)
		{
			if (getter != null && FoodUtilityPatch.WillFeedPatientAnimal(__result, p, getter))
			{
				__result = VFRSaveRestriction.Instance.foodRestiction.Allows(food);
			}
		}
	}
	
	
	// "temp"  WillEat
	[HarmonyPatch(typeof(FoodUtility), "WillEat_NewTemp", new Type[]
	{
		typeof(Pawn),
		typeof(Thing),
		typeof(Pawn),
		typeof(bool),
		typeof(bool)
	})]
	internal class FoodUtilityPatch3
	{
		[HarmonyPriority(200)]
		[HarmonyPostfix]
		public static void WillEatPatch1(ref bool __result, Pawn p, Thing food, Pawn getter)
		{
			if (getter != null && FoodUtilityPatch.WillFeedPatientAnimal(__result, p, getter))
			{
				__result = VFRSaveRestriction.Instance.foodRestiction.Allows(food);
			}
		}
	}
	[HarmonyPatch(typeof(FoodUtility), "WillEat_NewTemp", new Type[]
	{
		typeof(Pawn),
		typeof(ThingDef),
		typeof(Pawn),
		typeof(bool),
		typeof(bool)
	})]
	internal class FoodUtilityPatch4
	{
		[HarmonyPriority(200)]
		[HarmonyPostfix]
		public static void WillEatPatch2(ref bool __result, Pawn p, ThingDef food, Pawn getter)
		{
			if (getter != null && FoodUtilityPatch.WillFeedPatientAnimal(__result, p, getter))
			{
				__result = VFRSaveRestriction.Instance.foodRestiction.Allows(food);
			}
		}
	}
}
