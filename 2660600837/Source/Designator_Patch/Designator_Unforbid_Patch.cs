using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	public sealed class Designator_Unforbid_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Unforbid_Patch instance = new();

		public static Designator_Unforbid_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Unforbid_Patch()
		{
		}

		private enum UnforbidMode
		{
			All = 0,
			SameKind = Utility.SameKind,
		}
		private static UnforbidMode unforbidMode = UnforbidMode.All;

		public override void ClearMode()
		{
			unforbidMode = UnforbidMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (unforbidMode == UnforbidMode.SameKind)
			{
				return "FilterableDesignator.Unforbid.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Unforbid), nameof(Designator_Unforbid.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
				if (!__result.Accepted)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (unforbidMode == UnforbidMode.SameKind)
				{
					if (!firstTarget.HasTarget)
					{
						firstTarget.SetTarget(t);
					}
					if (FirstThing != null)
					{
						if (!FirstThing.CompareConstraintHash(t))
						{
							__result = false;
							return;
						}
						if (FoodUtility.GetFoodKind(FirstThing) != FoodUtility.GetFoodKind(t))
						{
							__result = false;
							return;
						}
					}
				}
			}
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			if (!Utility.ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			var floatMenuList = new List<(UnforbidMode, ThingDef, string, Texture)>
			{
				(UnforbidMode.All, null, "FilterableDesignator.Unforbid.FloatMenuOption.All".Translate(), null),
				(UnforbidMode.SameKind, null, "FilterableDesignator.Unforbid.FloatMenuOption.SameKind".Translate(), null),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				unforbidMode = mode;
			});
		}

		private protected override string MouseAttachmentText(Designator instance)
		{
			if (unforbidMode == UnforbidMode.SameKind)
			{
				if (FirstThing != null)
				{
					return "FilterableDesignator.Unforbid.Label.Thing".Translate(instance.defaultLabel, FirstThing.ThingLabelForSameKind());
				}
				return "FilterableDesignator.Unforbid.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.Label;
		}
	}
}
