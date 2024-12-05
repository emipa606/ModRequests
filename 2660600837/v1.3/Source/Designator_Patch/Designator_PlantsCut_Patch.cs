using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_PlantsCut_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_PlantsCut_Patch instance = new Designator_PlantsCut_Patch();

		public static Designator_PlantsCut_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_PlantsCut_Patch()
		{
		}

		private enum PlantsCutMode
		{
			All,
			Blighted,
			Dying,
			BluePrint,
			Wild,
			Similar,
		}
		private static PlantsCutMode plantsCutMode = PlantsCutMode.All;

		public override void ClearMode()
		{
			plantsCutMode = PlantsCutMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (plantsCutMode == PlantsCutMode.Blighted)
			{
				return "FilterableDesignator.PlantsCut.Label.Blighted".Translate(instance.defaultLabel);
			}
			else if (plantsCutMode == PlantsCutMode.Dying)
			{
				return "FilterableDesignator.PlantsCut.Label.Dying".Translate(instance.defaultLabel);
			}
			else if (plantsCutMode == PlantsCutMode.BluePrint)
			{
				return "FilterableDesignator.PlantsCut.Label.BluePrint".Translate(instance.defaultLabel);
			}
			else if (plantsCutMode == PlantsCutMode.Wild)
			{
				return "FilterableDesignator.PlantsCut.Label.Wild".Translate(instance.defaultLabel);
			}
			else if (plantsCutMode == PlantsCutMode.Similar)
			{
				return "FilterableDesignator.PlantsCut.Label.Similar".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_PlantsCut), nameof(Designator_PlantsCut.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
				if (!__result.Accepted)
				{
					return;
				}
				if (plantsCutMode == PlantsCutMode.All)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (t is Plant plant)
				{
					if (plantsCutMode == PlantsCutMode.Blighted && !plant.Blighted)
					{
						__result = "FilterableDesignator.PlantsCut.Message.MustDesignateBlighted".Translate();
					}
					else if (plantsCutMode == PlantsCutMode.Dying && !plant.Dying)
					{
						__result = "FilterableDesignator.PlantsCut.Message.MustDesignateDying".Translate();
					}
					else if (plantsCutMode == PlantsCutMode.BluePrint)
					{
						if (plant.Map is Map map)
						{
							if (map.cellIndices is CellIndices cellIndices)
							{
								int index = cellIndices.CellToIndex(plant.Position);
								var innerArray = map.blueprintGrid.InnerArray;
								if (innerArray[index] != null)
								{
									foreach (var blueprint in innerArray[index])
									{
										if (GenConstruct.BlocksConstruction(blueprint, plant))
										{
											return;
										}
									}
									__result = "FilterableDesignator.PlantsCut.Message.MustDesignateBluePrinted".Translate();
								}
								else
								{
									__result = "FilterableDesignator.PlantsCut.Message.MustDesignateBluePrinted".Translate();
								}
							}
						}
					}
					else if (plantsCutMode == PlantsCutMode.Wild)
					{
						if (plant.def.plant.IsTree || plant.def.plant.Sowable)
						{
							__result = "FilterableDesignator.PlantsCut.Message.MustDesignateWild".Translate();
						}
					}
					else if (plantsCutMode == PlantsCutMode.Similar)
					{
						foreach (object obj in Find.Selector.SelectedObjects)
						{
							if (obj is Plant selectedPlant)
							{
								if (plant.def.shortHash == selectedPlant.def.shortHash)
								{
									return;
								}
							}
						}
						__result = "FilterableDesignator.PlantsCut.Message.MustDesignateSimilar".Translate();
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
			ThingDef agariluxDef = (ThingDef)GenDefDatabase.GetDef(typeof(ThingDef), BackCompatibility.BackCompatibleDefName(typeof(ThingDef), "Agarilux"));
			var floatMenuList = new List<(PlantsCutMode, ThingDef, string, Texture2D)>
			{
				(PlantsCutMode.All, null, "FilterableDesignator.PlantsCut.FloatMenuOption.All".Translate().ToString(), instance.icon),
				(PlantsCutMode.Blighted, ThingDefOf.Blight, "FilterableDesignator.PlantsCut.FloatMenuOption.Blighted".Translate().ToString(), null),
				(PlantsCutMode.Dying, agariluxDef, "FilterableDesignator.PlantsCut.FloatMenuOption.Dying".Translate().ToString(), null),
				(PlantsCutMode.BluePrint, ThingDefOf.Wall.blueprintDef, "FilterableDesignator.PlantsCut.FloatMenuOption.BluePrint".Translate().ToString(), Utility.GetLinkedIcon(ThingDefOf.Wall.blueprintDef)),
				(PlantsCutMode.Wild, ThingDefOf.Plant_Grass, "FilterableDesignator.PlantsCut.FloatMenuOption.Wild".Translate().ToString(), null),
//				(PlantsCutMode.Similar, null, "FilterableDesignator.PlantsCut.FloatMenuOption.Similar".Translate().ToString(), null)
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				plantsCutMode = mode;
			});
		}
	}
}