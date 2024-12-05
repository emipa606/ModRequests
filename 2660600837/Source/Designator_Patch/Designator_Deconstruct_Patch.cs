using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	public sealed class Designator_Deconstruct_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Deconstruct_Patch instance = new();

		public static Designator_Deconstruct_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Deconstruct_Patch()
		{
		}

		private enum DeconstructMode
		{
			All = 0x0,
			WallOrDoor = 0x1,
			Conduit = 0x2,
			SameKind = Utility.SameKind,
			SameStuff = Utility.SameStuff,
			SameKindAndStuff = Utility.SameKindAndStuff,
		}
		private static DeconstructMode deconstructMode = DeconstructMode.All;

		public override void ClearMode()
		{
			deconstructMode = DeconstructMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (deconstructMode == DeconstructMode.WallOrDoor)
			{
				return "FilterableDesignator.Deconstruct.Label.WallOrDoor".Translate(instance.defaultLabel, ThingDefOf.Wall.label, ThingDefOf.Door.label);
			}
			else if (deconstructMode == DeconstructMode.Conduit)
			{
				return "FilterableDesignator.Deconstruct.Label.Conduit".Translate(instance.defaultLabel, ThingDefOf.PowerConduit.label);
			}
			else if (deconstructMode == DeconstructMode.SameKind)
			{
				return "FilterableDesignator.Deconstruct.Label.SameKind".Translate(instance.defaultLabel);
			}
			else if (deconstructMode == DeconstructMode.SameKindAndStuff)
			{
				return "FilterableDesignator.Deconstruct.Label.SameKindAndStuff".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Deconstruct), nameof(Designator_Deconstruct.CanDesignateThing))]
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
				if (deconstructMode == DeconstructMode.WallOrDoor)
				{
					if (t.def.shortHash != ThingDefOf.Wall.shortHash && t.def.shortHash != ThingDefOf.Door.shortHash)
					{
						__result = false;
						return;
					}
				}
				else if (deconstructMode == DeconstructMode.Conduit)
				{
					if (t.def.altitudeLayer != AltitudeLayer.Conduits)
					{
						__result = false;
						return;
					}
				}
				else if ((deconstructMode | DeconstructMode.SameKindAndStuff) != 0)
				{
					if (FirstThing != null)
					{
						if ((deconstructMode & DeconstructMode.SameKind) != 0)
						{
							if (t.def.shortHash != FirstThing.def.shortHash)
							{
								__result = false;
								return;
							}
						}
						if ((deconstructMode & DeconstructMode.SameStuff) != 0)
						{
							if (t.Stuff?.shortHash != FirstThing.Stuff?.shortHash)
							{
								__result = false;
								return;
							}
						}
					}
					else
					{
						firstTarget.SetTarget(t);
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
			var floatMenuList = new List<(DeconstructMode, ThingDef, string, Texture)>
			{
				(DeconstructMode.All, null, "FilterableDesignator.Deconstruct.FloatMenuOption.All".Translate().ToString(), instance.icon),
				(DeconstructMode.WallOrDoor, ThingDefOf.Wall, "FilterableDesignator.Deconstruct.FloatMenuOption.WallOrDoor".Translate(ThingDefOf.Wall.label, ThingDefOf.Door.label).ToString(), null),
				(DeconstructMode.Conduit, ThingDefOf.PowerConduit, "FilterableDesignator.Deconstruct.FloatMenuOption.Conduit".Translate(ThingDefOf.PowerConduit.label).ToString(), null),
				(DeconstructMode.SameKind, null, "FilterableDesignator.Deconstruct.FloatMenuOption.SameKind".Translate().ToString(), null),
				(DeconstructMode.SameKindAndStuff, null, "FilterableDesignator.Deconstruct.FloatMenuOption.SameKindAndStuff".Translate().ToString(), null),
//				(DeconstructMode.Similar, null, "FilterableDesignator.Deconstruct.FloatMenuOption.Similar".Translate().ToString(), null)
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				deconstructMode = mode;
			});
		}

		private protected override string MouseAttachmentText(Designator instance)
		{
			if ((deconstructMode | DeconstructMode.SameKindAndStuff) != 0)
			{
				if (FirstThing != null)
				{
					var thingName = (deconstructMode == DeconstructMode.SameKindAndStuff) ? FirstThing.ThingLabelForSameKind() : FirstThing.def.label;
					return "FilterableDesignator.Deconstruct.MouseAttachment.Thing".Translate(instance.defaultLabel, thingName);
				}
			}
			return instance.Label;
		}
	}
}
