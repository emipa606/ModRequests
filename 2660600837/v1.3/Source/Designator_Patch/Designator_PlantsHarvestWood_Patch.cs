using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_PlantsHarvestWood_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_PlantsHarvestWood_Patch instance = new Designator_PlantsHarvestWood_Patch();

		public static Designator_PlantsHarvestWood_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_PlantsHarvestWood_Patch()
		{
		}

		private enum PlantsHarvestWoodMode
		{
			All = 0,
			Above75 = 75,
			Above85 = 85,
			Above95 = 95,
			FullGrown = 100,
			Extract = 999,
//			Similar = 9999,
		}
		private static PlantsHarvestWoodMode plantsHarvestWoodMode = PlantsHarvestWoodMode.All;

		public override void ClearMode()
		{
			plantsHarvestWoodMode = PlantsHarvestWoodMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (plantsHarvestWoodMode == PlantsHarvestWoodMode.All)
			{
				return instance.defaultLabel;
			}
			else if (plantsHarvestWoodMode == PlantsHarvestWoodMode.FullGrown)
			{
				return "FilterableDesignator.PlantsHarvestWood.Label.FullGrown".Translate(instance.defaultLabel);
			}
			else if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Extract)
			{
				return "DesignatorExtractTree".Translate();
			}
/*
			else if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Similar)
			{
				return "FilterableDesignator.PlantsHarvestWood.Label.Similar".Translate();
			}
*/
			return "FilterableDesignator.PlantsHarvestWood.Label.MinGrowth".Translate(instance.defaultLabel, ((int)plantsHarvestWoodMode + "%").ToString());
		}

		[HarmonyPatch(typeof(Designator_Plants), nameof(Designator_Plants.CanDesignateCell))]
		public class CanDesignateCell_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Designator_Plants __instance, ref AcceptanceReport __result, IntVec3 c)
			{
				if (__instance is Designator_PlantsHarvestWood instance)
				{
					if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Extract)
					{
						if (!c.InBounds(Find.CurrentMap))
						{
							__result = AcceptanceReport.WasRejected;
							return false;
						}
						foreach (Thing thing in c.GetThingList(Find.CurrentMap))
						{
							if (instance.CanDesignateThing(thing).Accepted)
							{
								__result = true;
								return false;
							}
						}
						__result = AcceptanceReport.WasRejected;
						return false;
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Designator_PlantsHarvestWood), nameof(Designator_PlantsHarvestWood.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
/*	"Extract tree"ÉÇÅ[ÉhÇÃÇ∆Ç´ÇÕî∞çÃÇÃëŒè€Ç…Ç»ÇÁÇ»Ç¢àÁê¨ìxÇÃé˜ñÿÇ‡ëŒè€Ç…Ç∑ÇÈ
				if (!__result.Accepted)
				{
					return;
				}
*/
				if (plantsHarvestWoodMode == PlantsHarvestWoodMode.All)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (__result.Accepted)
				{
					if (t is Plant plant)
					{
						if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Extract)
						{
							if (plant.def.Minifiable && plant.def.plant.IsTree && Find.CurrentMap.designationManager.DesignationOn(plant, DesignationDefOf.ExtractTree) is null)
							{
								;
							}
							else
							{
								__result = AcceptanceReport.WasRejected;
							}
						}
/*
						else if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Similar)
						{
							if (t is Plant plant)
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
								__result = "MessageMustDesignateHarvestableWood".Translate();
							}
						}
*/
						else if (plantsHarvestWoodMode == PlantsHarvestWoodMode.FullGrown)
						{
							if (plant.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Mature)
							{
								__result = "MessageMustDesignateHarvestableWoodWood".Translate();
							}
						}
						else
						{
							if (plant.Growth * 100 < (float)plantsHarvestWoodMode)
							{
								__result = "MessageMustDesignateHarvestableWood".Translate();
							}
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Designator_PlantsHarvestWood), nameof(Designator_PlantsHarvestWood.PossiblyWarnPlayerOnDesignatingTreeCut))]
		public class PossiblyWarnPlayerOnDesignatingTreeCut_Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				if (plantsHarvestWoodMode == PlantsHarvestWoodMode.Extract)
				{
					return false;
				}
				return true;
			}
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			if (!Utility.ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			var icon1 = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood");
			var icon2 = ContentFinder<Texture2D>.Get("UI/Designators/ExtractTree");
			var floatMenuList = new List<(PlantsHarvestWoodMode, ThingDef, string, Texture2D)>
			{
				(PlantsHarvestWoodMode.All, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.All".Translate().ToString(), icon1),
				(PlantsHarvestWoodMode.Above75, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate((int)PlantsHarvestWoodMode.Above75 + "%")).ToString(), icon1),
				(PlantsHarvestWoodMode.Above85, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate((int)PlantsHarvestWoodMode.Above85 + "%")).ToString(), icon1),
				(PlantsHarvestWoodMode.Above95, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.MinGrowth".Translate("PercentGrowth".Translate((int)PlantsHarvestWoodMode.Above95 + "%")).ToString(), icon1),
				(PlantsHarvestWoodMode.FullGrown, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.FullGrown".Translate().ToString(), icon1),
				(PlantsHarvestWoodMode.Extract, null, "FilterableDesignator.PlantsHarvestWood.FloatMenuOption.Extract".Translate().ToString(), icon2),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				plantsHarvestWoodMode = mode;
				if (instance is Designator_Plants instancePlants)
				{
					ref var designationDef = ref AccessTools.FieldRefAccess<Designator_Plants, DesignationDef>(instancePlants, "designationDef");
					if (mode == PlantsHarvestWoodMode.Extract)
					{
						designationDef = DesignationDefOf.ExtractTree;
						instancePlants.soundSucceeded = SoundDefOf.Designate_ExtractTree;
						variableIcon = ContentFinder<Texture2D>.Get("UI/Designators/ExtractTree");
					}
					else
					{
						designationDef = DesignationDefOf.HarvestPlant;
						instancePlants.soundSucceeded = SoundDefOf.Designate_Harvest;
						variableIcon = ContentFinder<Texture2D>.Get("UI/Designators/HarvestWood");
					}
				}
			});
		}
	}
}