using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_Haul_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Haul_Patch instance = new Designator_Haul_Patch();

		public static Designator_Haul_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Haul_Patch()
		{
		}

		private enum HaulMode
		{
			All,
			Specific,
//			Similar,
			SameKind = Utility.SameKind,
		}
		private static HaulMode haulMode = HaulMode.All;

		public override void ClearMode()
		{
			haulMode = HaulMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (haulMode == HaulMode.Specific)
			{
				return "FilterableDesignator.Haul.Label.Specific".Translate(instance.defaultLabel, filteringStuff?.label ?? "");
			}
/*
			else if (haulMode == HaulMode.Similar)
			{
				return "FilterableDesignator.Haul.Label.Similar".Translate(instance.defaultLabel);
			}
*/
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Haul), nameof(Designator_Haul.CanDesignateThing))]
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
				if (haulMode == HaulMode.Specific)
				{
					if (t.def.shortHash != filteringStuff.shortHash)
					{
						__result = "FilterableDesignator.Haul.Message.MustDesignateFilteredChunk".Translate();
					}
				}
/*
				else if (haulMode == HaulMode.Similar)
				{
					foreach (object obj in Find.Selector.SelectedObjects)
					{
						if (obj is Thing selectedThing)
						{
							if (t.def.shortHash == selectedThing.def.shortHash)
							{
								return;
							}
						}
					}
					__result = "FilterableDesignator.Haul.Message.MustDesignateSimilar".Translate();
				}
*/
			}
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			if (!Utility.ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			var floatMenuList = new List<(HaulMode, ThingDef, string, Texture2D)> { };
			{
				// All
				floatMenuList.Add((HaulMode.All, null, "FilterableDesignator.Haul.FloatMenuOption.All".Translate().ToString(), instance.icon));
				// Specific
				Map map = Find.CurrentMap;
				foreach (var chunk in from cell in map.cellsInRandomOrder.GetAll()
									  select cell.GetFirstHaulable(map) into haulable
									  where ((!haulable?.def.alwaysHaulable) ?? false) && (haulable?.def.designateHaulable ?? false)
									  group haulable.def by haulable.def.shortHash into defGroup
									  orderby defGroup.First().stuffProps?.commonality ?? float.PositiveInfinity descending, defGroup.First().building?.mineableThing?.BaseMarketValue ?? 0
									  select defGroup.First())
				{
					floatMenuList.Add((HaulMode.Specific, chunk, "FilterableDesignator.Haul.FloatMenuOption.Specific".Translate(chunk.label), null));
				}
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				haulMode = mode;
				filteringStuff = thingDef;
			});
		}
	}
}
