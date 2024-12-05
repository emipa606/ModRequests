using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_PlantsHarvest_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_PlantsHarvest_Patch instance = new();

		public static Designator_PlantsHarvest_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_PlantsHarvest_Patch()
		{
		}

		private enum PlantsHarvestMode
		{
			All = 0,
			Above75 = 75,
			Above85 = 85,
			Above95 = 95,
			FullGrown = 100,
			EdibleAll = 0 + 0x80,
			EdibleAbove75 = 75 + 0x80,
			EdibleAbove85 = 85 + 0x80,
			EdibleAbove95 = 95 + 0x80,
			EdibleFullGrown = 100 + 0x80,
			Similar = 9999,
		}
		private static PlantsHarvestMode plantsHarvestMode = PlantsHarvestMode.All;

		private static bool IsModeEdibleOnly(PlantsHarvestMode mode)
		{
			return ((int)mode & 0x80) != 0;
		}

		private static int HarvestableThreshold(PlantsHarvestMode mode)
		{
			return ((int)mode & 0x7F);
		}

		public override void ClearMode()
		{
			plantsHarvestMode = PlantsHarvestMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (plantsHarvestMode == PlantsHarvestMode.All)
			{
				return instance.defaultLabel;
			}
			else if (plantsHarvestMode == PlantsHarvestMode.FullGrown)
			{
				return "FilterableDesignator.PlantsHarvest.Label.FullGrown".Translate(instance.defaultLabel);
			}
			else if (plantsHarvestMode == PlantsHarvestMode.Similar)
			{
				return "FilterableDesignator.PlantsHarvest.Label.Similar".Translate();
			}
			else if (IsModeEdibleOnly(plantsHarvestMode))
			{
				if (plantsHarvestMode == PlantsHarvestMode.EdibleAll)
				{
					return "FilterableDesignator.PlantsHarvest.Label.Edible".Translate(instance.defaultLabel);
				}
				else if (plantsHarvestMode == PlantsHarvestMode.EdibleFullGrown)
				{
					return "FilterableDesignator.PlantsHarvest.Label.FullGrownEdible".Translate(instance.defaultLabel);
				}
				return "FilterableDesignator.PlantsHarvest.Label.MinGrowthEdible".Translate(instance.defaultLabel, HarvestableThreshold(plantsHarvestMode) + "%");
			}
			return "FilterableDesignator.PlantsHarvest.Label.MinGrowth".Translate(instance.defaultLabel, HarvestableThreshold(plantsHarvestMode) + "%");
		}

		[HarmonyPatch(typeof(Designator_PlantsHarvest), nameof(Designator_PlantsHarvest.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
				if (IsReverseDesignator())
				{
					return;
				}
				if (__result.Accepted)
				{
					if (t is Plant plant)
					{
						if (plantsHarvestMode == PlantsHarvestMode.Similar)
						{
							bool found = false;
							foreach (object obj in Find.Selector.SelectedObjects)
							{
								if (obj is Plant selectedPlant)
								{
									if (plant.def.shortHash == selectedPlant.def.shortHash)
									{
										found = true;
										break;
									}
								}
							}
							if (!found)
							{
								__result = "MessageMustDesignateHarvestable".Translate();
								return;
							}
						}
						else if (plantsHarvestMode == PlantsHarvestMode.FullGrown || plantsHarvestMode == PlantsHarvestMode.EdibleFullGrown)
						{
							if (plant.LifeStage != PlantLifeStage.Mature)
							{
								__result = "MessageMustDesignateHarvestable".Translate();
								return;
							}
						}
						else
						{
							if (plant.Growth * 100 < HarvestableThreshold(plantsHarvestMode))
							{
								__result = "MessageMustDesignateHarvestable".Translate();
								return;
							}
						}

						if (IsModeEdibleOnly(plantsHarvestMode))
						{
							if (plant.def.plant.harvestedThingDef is ThingDef crop && crop.ingestible is IngestibleProperties ingestible && (ingestible.foodType & FoodTypeFlags.OmnivoreHuman) != 0)
							{
								return;
							}
							__result = "FilterableDesignator.PlantsHarvest.Message.MustDesignateEdible".Translate();
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
			ThingDef cropDef = (ThingDef)GenDefDatabase.GetDef(typeof(ThingDef), BackCompatibility.BackCompatibleDefName(typeof(ThingDef), "Plant_Corn"));
			var floatMenuList = new List<(PlantsHarvestMode, ThingDef, string, Texture)>
			{
				(PlantsHarvestMode.All, null, "FilterableDesignator.PlantsHarvest.FloatMenuOption.All".Translate().ToString(), instance.icon),
				(PlantsHarvestMode.Above75, null, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.Above75) + "%")).ToString(), instance.icon),
				(PlantsHarvestMode.Above85, null, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.Above85) + "%")).ToString(), instance.icon),
				(PlantsHarvestMode.Above95, null, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.Above95) + "%")).ToString(), instance.icon),
				(PlantsHarvestMode.FullGrown, null, "FilterableDesignator.PlantsHarvest.FloatMenuOption.FullGrown".Translate().ToString(), instance.icon),
				(PlantsHarvestMode.EdibleAll, cropDef, "FilterableDesignator.PlantsHarvest.FloatMenuOption.Edible".Translate().ToString(), null),
				(PlantsHarvestMode.EdibleAbove75, cropDef, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.EdibleAbove75) + "%")).ToString(), null),
				(PlantsHarvestMode.EdibleAbove85, cropDef, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.EdibleAbove85) + "%")).ToString(), null),
				(PlantsHarvestMode.EdibleAbove95, cropDef, "FilterableDesignator.PlantsHarvest.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate(HarvestableThreshold(PlantsHarvestMode.EdibleAbove95) + "%")).ToString(), null),
				(PlantsHarvestMode.EdibleFullGrown, cropDef, "FilterableDesignator.PlantsHarvest.FloatMenuOption.FullGrownEdible".Translate().ToString(), null),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				plantsHarvestMode = mode;
			});
		}
	}
}