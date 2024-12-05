using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_Mine_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Mine_Patch instance = new Designator_Mine_Patch();

		public static Designator_Mine_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Mine_Patch()
		{
		}

		private enum MineMode
		{
			All,
			Specific,
			RoughRock,
			Similar,
			SameKind = Utility.SameKind,
		}
		private static MineMode mineMode = MineMode.All;

		public override void ClearMode()
		{
			mineMode = MineMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (mineMode == MineMode.Specific)
			{
				return "FilterableDesignator.Mine.Label.Specific".Translate(instance.defaultLabel, filteringStuff?.label ?? "");
			}
			else if (mineMode == MineMode.RoughRock)
			{
				return "FilterableDesignator.Mine.Label.RoughRock".Translate(instance.defaultLabel);
			}
			else if (mineMode == MineMode.Similar)
			{
				return "FilterableDesignator.Mine.Label.Similar".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Mine), nameof(Designator_Mine.CanDesignateCell))]
		public class CanDesignateCell_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, IntVec3 c)
			{
				if (__result.Accepted)
				{
					if (mineMode != MineMode.All)
					{
						if (c.Fogged(Find.CurrentMap))
						{
							__result = AcceptanceReport.WasRejected;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Designator_Mine), nameof(Designator_Mine.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
				if (!__result.Accepted)
				{
					return;
				}
				if (mineMode == MineMode.All)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (t is Building building)
				{
					if (mineMode == MineMode.Specific && building.def.shortHash != filteringStuff.shortHash)
					{
						__result = "FilterableDesignator.Mine.Message.MustDesignateFilteredStuff".Translate();
					}
					else if (mineMode == MineMode.RoughRock && ((!building.def.building?.isNaturalRock) ?? false))
					{
						__result = "FilterableDesignator.Mine.Message.MustDesignateRoughRocks".Translate();
					}
					else if (mineMode == MineMode.Similar)
					{
						bool found = false;
						foreach (object obj in Find.Selector.SelectedObjects)
						{
							if (obj is Building selectedBuilding)
							{
								if (building.def.shortHash == selectedBuilding.def.shortHash)
								{
									found = true;
									break;
								}
							}
						}
						if (!found)
						{
							__result = "FilterableDesignator.Mine.Message.MustDesignateSimilarRocks".Translate();
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
			var floatMenuList = new List<(MineMode, ThingDef, string, Texture2D)> { };
			{
				// All
				floatMenuList.Add((MineMode.All, null, "FilterableDesignator.Mine.FloatMenuOption.All".Translate().ToString(), instance.icon));
				// Specific
				Map map = Find.CurrentMap;
				foreach (var rock in from cell in map.cellsInRandomOrder.GetAll()
									 select cell.GetFirstMineable(map) into minerable
									 where minerable?.def.building?.isNaturalRock ?? false
									 group minerable.def by minerable.def.shortHash into defGroup
									 orderby defGroup.First().stuffProps?.commonality ?? float.PositiveInfinity descending, defGroup.First().building?.mineableThing?.BaseMarketValue ?? 0
									 select defGroup.First())
				{
					floatMenuList.Add((MineMode.Specific, rock, "FilterableDesignator.Mine.FloatMenuOption.Specific".Translate(rock.label), Utility.GetLinkedIcon(rock)));
				}
				// RoughRock
				floatMenuList.Add((MineMode.RoughRock, null, "FilterableDesignator.Mine.FloatMenuOption.RoughRock".Translate(), null));
				// Similar
//				floatMenuList.Add((MineMode.Similar, null, "FilterableDesignator.Mine.FloatMenuOption.Similar".Translate().ToString(), null));
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				mineMode = mode;
				filteringStuff = thingDef;
			});
		}
	}
}
