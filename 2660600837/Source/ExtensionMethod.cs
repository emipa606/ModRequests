using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilterableDesignator.ExtensionMethod
{
	static class ExtensionMethod
	{
		private static int? GetConstraintHashForThing(Thing thing, bool compareStuff = true)
		{
			if (thing is null)
			{
				return null;
			}
			int hash = thing.def.shortHash;
			if (compareStuff && thing.Stuff != null)
				unchecked
				{
//					hash += thing.Stuff.shortHash * 31;
					hash ^= thing.Stuff.shortHash;
				}
			return hash;
		}

		public static bool CompareConstraintHash(this Thing thing1, Thing thing2)
		{
			return GetConstraintHashForThing(thing1) == GetConstraintHashForThing(thing2);
		}

		public static bool CompareHash(this Designation designation1, Designation designation2, bool compareThing = true)
		{
			if (designation1?.def.shortHash != designation2?.def.shortHash)
			{
				return false;
			}
			if (compareThing)
			{
				if (GetConstraintHashForThing(designation1?.target.Thing) != GetConstraintHashForThing(designation2?.target.Thing))
				{
					return false;
				}
			}
			return true;
		}

		public static Designator_Patch PatchInstance(this Command instance)
		{
			if (instance is Designator_Cancel)
			{
				return Designator_Cancel_Patch.Instance;
			}
			if (instance is Designator_Deconstruct)
			{
				return Designator_Deconstruct_Patch.Instance;
			}
			if (instance is Designator_ExtractTree)
			{
				return Designator_ExtractTree_Patch.Instance;
			}
			if (instance is Designator_Mine)
			{
				return Designator_Mine_Patch.Instance;
			}
			if (instance is Designator_SmoothSurface)
			{
				return Designator_SmoothSurface_Patch.Instance;
			}
			if (instance is Designator_Haul)
			{
				return Designator_Haul_Patch.Instance;
			}
			if (instance is Designator_PlantsCut)
			{
				return Designator_PlantsCut_Patch.Instance;
			}
			if (instance is Designator_PlantsHarvest)
			{
				return Designator_PlantsHarvest_Patch.Instance;
			}
			if (instance is Designator_PlantsHarvestWood)
			{
				return Designator_PlantsHarvestWood_Patch.Instance;
			}
			if (instance is Designator_Tame)
			{
				return Designator_Tame_Patch.Instance;
			}
			if (instance is Designator_Unforbid)
			{
				return Designator_Unforbid_Patch.Instance;
			}
			if (instance is Designator_Forbid)
			{
				return Designator_Forbid_Patch.Instance;
			}
			return null;
		}

		public static string MouseAttachmentText(this Designator instance)
		{
			string text = instance.Label;
			if (instance.PatchInstance() is Designator_Patch patchInstance)
			{
				MethodInfo MouseAttachmentTextInfo = AccessTools.Method(patchInstance.GetType(), "MouseAttachmentText");
				text = (string)MouseAttachmentTextInfo?.Invoke(patchInstance, new object[] { instance });
			}
			return text;
		}

		public static string ThingLabelForSameKind(this Thing t)
		{
			if (t is null)
			{
				return "";
			}
			ThingStyleDef styleDef = t.StyleDef;
			string text = (styleDef is null || styleDef.overrideLabel.NullOrEmpty()) ? GenLabel.ThingLabel(t.def, t.Stuff) : styleDef.overrideLabel;
			if (FoodUtility.GetFoodKind(t) == FoodKind.Meat)
			{
				text += " (" + "MealKindMeat".Translate() + ")";
			}
			else if (FoodUtility.GetFoodKind(t) == FoodKind.NonMeat)
			{
				text += " (" + "MealKindVegetarian".Translate() + ")";
			}
			return text;
		}
	}
}
