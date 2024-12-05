using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	public sealed class Designator_Forbid_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Forbid_Patch instance = new();

		public static Designator_Forbid_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Forbid_Patch()
		{
		}

		private enum ForbidMode
		{
			All = 0,
			SameKind = Utility.SameKind,
		}
		private static ForbidMode forbidMode = ForbidMode.All;

		public override void ClearMode()
		{
			forbidMode = ForbidMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (forbidMode == ForbidMode.SameKind)
			{
				return "FilterableDesignator.Forbid.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Forbid), nameof(Designator_Forbid.CanDesignateThing))]
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
				if (forbidMode == ForbidMode.SameKind)
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
			var floatMenuList = new List<(ForbidMode, ThingDef, string, Texture)>
			{
				(ForbidMode.All, null, "FilterableDesignator.Forbid.FloatMenuOption.All".Translate().ToString(), null),
				(ForbidMode.SameKind, null, "FilterableDesignator.Forbid.FloatMenuOption.SameKind".Translate().ToString(), null),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				forbidMode = mode;
			});
		}

		private protected override string MouseAttachmentText(Designator instance)
		{
			if (forbidMode == ForbidMode.SameKind)
			{
				if (FirstThing != null)
				{
					return "FilterableDesignator.Forbid.Label.Thing".Translate(instance.defaultLabel, FirstThing.ThingLabelForSameKind());
				}
				return "FilterableDesignator.Forbid.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.Label;
		}
	}
}
