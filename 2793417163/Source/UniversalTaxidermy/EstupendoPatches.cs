using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace EstupendoBase.Patches
{
	class EstupendoPatch
	{
		[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
		public class RegisterIngredients
		{
			[HarmonyPostfix]
			public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, List<Thing> ingredients)
			{
				foreach( Thing t in values )
				{
					if( t is IDisplayable retainer && retainer.WantsToRetain )
					{
						IEnumerable<Thing> extras = retainer.RegisterIngredients(ingredients);
						foreach( Thing extra in extras )
						{
							yield return extra;
						}
					}
					yield return t;
				}
				yield break;
			}
		}

		[HarmonyPatch(typeof(TargetingParameters), nameof(TargetingParameters.CanTarget))]
		public class DontTargetDisplayables
		{
			[HarmonyPostfix]
			public static void Postfix(ref bool __result, TargetInfo targ)
			{
				if( __result && targ.HasThing && targ.Thing is IDisplayable displayable )
				{
					if( displayable.IsDisplayed ) __result = false;
				}
			}
		}

		[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.GetPosture))]
		public class FakeStandingInDisplay
		{
			[HarmonyPostfix]
			public static void Postfix(ref PawnPosture __result, Pawn p)
			{
				if( p.ParentHolder is UniversalTaxidermy.Thing_TaxidermyMount mount )
				{
					__result = PawnPosture.Standing;
				}
			}
		}
	}
}
