using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	public sealed class Designator_Cancel_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Cancel_Patch instance = new Designator_Cancel_Patch();

		public static Designator_Cancel_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Cancel_Patch()
		{
		}

		private enum CancelMode
		{
			All,
			Floor,
			Conduit,
			SameKind = Utility.SameKind,
		}
		private static CancelMode cancelMode = CancelMode.All;

		public override void ClearMode()
		{
			cancelMode = CancelMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (cancelMode == CancelMode.Floor)
			{
				return "FilterableDesignator.Cancel.Label.Floor".Translate(instance.defaultLabel);
			}
			else if (cancelMode == CancelMode.Conduit)
			{
				return "FilterableDesignator.Cancel.Label.Thing".Translate(instance.defaultLabel, ThingDefOf.PowerConduit.label);
			}
			else if (cancelMode == CancelMode.SameKind)
			{
				return "FilterableDesignator.Cancel.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Cancel), nameof(Designator_Cancel.CanDesignateCell))]
		public class CanDesignateCell_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Designator_Cancel __instance, ref AcceptanceReport __result, IntVec3 c)
			{
				if (__result.Accepted)
				{
					var manager = Find.CurrentMap.designationManager;
					if (!firstTarget.HasTarget)
					{
						// 同じセルに複数の対象がある場合は､床と電力管の優先度を下げる
						var edifice = c.GetEdifice(Find.CurrentMap);
						var buildableDef = edifice?.def.entityDefToBuild ?? edifice?.def;
						if (buildableDef != null)
						{
							if (buildableDef.designationCategory != DesignationCategoryDefOf.Floors && buildableDef.altitudeLayer != AltitudeLayer.Conduits)
							{
								firstTarget.SetTarget(edifice);
							}
						}
						else
						{
							foreach (var thing in c.GetThingList(Find.CurrentMap))
							{
								if (thing.def.entityDefToBuild is ThingDef thingDef)
								{
									if (thingDef.designationCategory != DesignationCategoryDefOf.Floors && thingDef.altitudeLayer != AltitudeLayer.Conduits)
									{
										firstTarget.SetTarget(thing);
										break;
									}
								}
							}
						}
						if (!firstTarget.HasTarget)
						{
							foreach (var designation1 in manager.AllDesignationsAt(c))
							{
								if (designation1.target.Thing is Thing thing)
								{
									if (thing.def.designationCategory != DesignationCategoryDefOf.Floors && thing.def.altitudeLayer != AltitudeLayer.Conduits)
									{
										firstTarget.SetTarget(designation1);
										break;
									}
								}
							}
						}
						if (!firstTarget.HasTarget)
						{
							if (manager.AllDesignationsAt(c)?.FirstOrFallback() is Designation designation2)
							{
								firstTarget.SetTarget(designation2);
							}
						}
					}
					if (__instance.PatchInstance() is Designator_Cancel_Patch patchInstance)
					{
						foreach (var designation in manager.AllDesignationsAt(c))
						{
							if (patchInstance.FilteredDesignationAt(c, designation) != null)
							{
								return;
							}
						}
					}
					foreach (var thing in c.GetThingList(Find.CurrentMap))
					{
						if (__instance.CanDesignateThing(thing))
						{
							return;
						}
					}
					__result = AcceptanceReport.WasRejected;
				}
			}
		}

		[HarmonyPatch(typeof(Designator_Cancel), nameof(Designator_Cancel.CanDesignateThing))]
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
				var buildableDef = t.def.entityDefToBuild ?? t.def;
				if (cancelMode == CancelMode.Floor)
				{
					if (buildableDef.designationCategory != DesignationCategoryDefOf.Floors)
					{
						__result = false;
						return;
					}
				}
				else if (cancelMode == CancelMode.Conduit)
				{
					if (buildableDef.altitudeLayer != AltitudeLayer.Conduits)
					{
						__result = false;
						return;
					}
				}
				else if (cancelMode == CancelMode.SameKind)
				{
					var manager = Find.CurrentMap.designationManager;
					if (FirstDesignation != null)
					{
						if (!FirstDesignation.CompareHash(manager.DesignationOn(t)))
						{
							__result = AcceptanceReport.WasRejected;
							return;
						}
					}
					if (!firstTarget.HasTarget)
					{
						// 同じセルに複数の対象がある場合は､床と電力管の優先度を下げる
						foreach (var thing in t.Position.GetThingList(Find.CurrentMap))
						{
							if (thing.def.entityDefToBuild is ThingDef thingDef)
							{
								if (thingDef.designationCategory != DesignationCategoryDefOf.Floors && thingDef.altitudeLayer != AltitudeLayer.Conduits)
								{
									firstTarget.SetTarget(thing);
									break;
								}
							}
						}
						if (!firstTarget.HasTarget)
						{
							firstTarget.SetTarget(t);
						}
					}
					if (FirstThing != null)
					{
						if (!FirstThing.CompareConstraintHash(t))
						{
							__result = AcceptanceReport.WasRejected;
							return;
						}
					}
				}
			}
		}

		private Designation FilteredDesignationAt(IntVec3 c, Designation designation)
		{
			var manager = Find.CurrentMap.designationManager;
			if (cancelMode == CancelMode.Floor)
			{
				if (designation.def.targetType == TargetType.Cell)
				{
					if (!c.Filled(Find.CurrentMap))
					{
						return manager.DesignationAt(c, designation.def);
					}
				}
			}
			else if (cancelMode == CancelMode.Conduit)
			{
				if (designation.target.Thing is Thing thing)
				{
					var buildableDef = thing.def.entityDefToBuild ?? thing.def;
					if (buildableDef.altitudeLayer == AltitudeLayer.Conduits)
					{
						return manager.DesignationOn(thing, designation.def);
					}
				}
			}
			else if (cancelMode == CancelMode.SameKind)
			{
				if (FirstDesignation != null)
				{
					if (FirstDesignation.CompareHash(designation))
					{
						if (designation.def.targetType == TargetType.Thing)
						{
							return manager.DesignationOn(designation.target.Thing, designation.def);
						}
						return manager.DesignationAt(c, designation.def);
					}
				}
			}
			else
			{
				if (designation.def.targetType == TargetType.Thing)
				{
					return manager.DesignationOn(designation.target.Thing, designation.def);
				}
				return manager.DesignationAt(c, designation.def);
			}
			return null;
		}

		[HarmonyPatch(typeof(Designator_Cancel), nameof(Designator_Cancel.DesignateSingleCell))]
		public class DesignateSingleCell_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Designator_Cancel __instance, IntVec3 c)
			{
				var map = Find.CurrentMap;
				var cancelableDesignations = from x in map.designationManager.AllDesignationsAt(c)
											 where x.def != DesignationDefOf.Plan && x.def.designateCancelable
											 select x;
				var patchInstance = __instance.PatchInstance() as Designator_Cancel_Patch;
				foreach (Designation designation in cancelableDesignations.Reverse())
				{
					if (cancelMode == CancelMode.Floor)
					{
						if (patchInstance?.FilteredDesignationAt(c, designation) is null)
						{
							continue;
						}
					}
					else if (cancelMode == CancelMode.Conduit)
					{
						if (designation.target.HasThing && designation.target.Thing.def.altitudeLayer != AltitudeLayer.Conduits)
						{
							continue;
						}
					}
					else if (cancelMode == CancelMode.SameKind)
					{
						if (FirstDesignation != null)
						{
							if (!FirstDesignation.CompareHash(designation))
							{
								continue;
							}
						}
						if (FirstThing != null)
						{
							continue;
						}
					}
					map.designationManager.RemoveDesignation(designation);
				}
				foreach (var thing in c.GetThingList(map).AsEnumerable().Reverse())
				{
					if (__instance.CanDesignateThing(thing).Accepted)
					{
						__instance.DesignateThing(thing);
					}
				}
				return false;
			}
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			if (!Utility.ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			var floatMenuList = new List<(CancelMode, ThingDef, string, Texture2D)>
			{
				(CancelMode.All, null, "FilterableDesignator.Cancel.FloatMenuOption.All".Translate().ToString(), instance.icon),
				(CancelMode.Floor, TerrainDefOf.TileSandstone.blueprintDef, "FilterableDesignator.Cancel.FloatMenuOption.Floor".Translate().ToString(), null),
				(CancelMode.Conduit, ThingDefOf.PowerConduit.blueprintDef, "FilterableDesignator.Cancel.FloatMenuOption.Thing".Translate(ThingDefOf.PowerConduit.label).ToString(), Utility.GetLinkedIcon(ThingDefOf.PowerConduit.blueprintDef)),
				(CancelMode.SameKind, null, "FilterableDesignator.Cancel.FloatMenuOption.SameKind".Translate().ToString(), null),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				cancelMode = mode;
			});
		}

		private protected override string MouseAttachmentText(Designator instance)
		{
			if (cancelMode == CancelMode.SameKind)
			{
				if (FirstDesignation != null)
				{
					TaggedString designatorName = "指定";
					if (FirstDesignation.def.HasModExtension<DesignatorLabel>())
					{
						var designatorLabel = FirstDesignation.def.GetModExtension<DesignatorLabel>().designatorLabel;
						if (designatorLabel != "")
						{
							designatorName = ("Designator" + designatorLabel).Translate();
						}
					}
					if (FirstThing != null)
					{
						return "FilterableDesignator.Cancel.MouseAttachment.Thing".Translate(instance.defaultLabel, designatorName, FirstThing.ThingLabelForSameKind());
					}
					else
					{
						var cell = FirstDesignation.target.Cell;
						var edifice = cell.GetEdifice(Find.CurrentMap);
						if (edifice?.Position == cell && FirstDesignation.def.targetType == TargetType.Thing)
						{
							return "FilterableDesignator.Cancel.MouseAttachment.Thing".Translate(instance.defaultLabel, designatorName, edifice.def.label);
						}
						else
						{
							var terrain = Find.CurrentMap.terrainGrid.TerrainAt(cell);
							return "FilterableDesignator.Cancel.MouseAttachment.Thing".Translate(instance.defaultLabel, designatorName, terrain.label);
						}
					}
				}
				else if (FirstThing != null)
				{
					return "FilterableDesignator.Cancel.Label.Thing".Translate(instance.defaultLabel, FirstThing.ThingLabelForSameKind());
				}
				return "FilterableDesignator.Cancel.Label.SameKind".Translate(instance.defaultLabel);
			}
			return instance.Label;
		}

		[HarmonyPatch(typeof(Designator_Cancel), nameof(Designator_Cancel.RenderHighlight))]
		public class RenderHighlight_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Designator_Cancel __instance, List<IntVec3> dragCells)
			{
				var manager = Find.CurrentMap.designationManager;
				var patchInstance = __instance.PatchInstance() as Designator_Cancel_Patch;

				var seenThings = new HashSet<Thing>();
				foreach (IntVec3 dragCell in dragCells)
				{
					foreach (Designation designation in manager.AllDesignationsAt(dragCell))
					{
						if (designation.def.targetType == TargetType.Cell && patchInstance?.FilteredDesignationAt(dragCell, designation) != null)
						{
							Graphics.DrawMesh(MeshPool.plane10, dragCell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays.AltitudeFor()), Quaternion.identity, DesignatorUtility.DragHighlightCellMat, 0);
							break;
						}
					}
					foreach (Thing thing in dragCell.GetThingList(Find.CurrentMap))
					{
						if (!seenThings.Contains(thing) && __instance.CanDesignateThing(thing).Accepted)
						{
							Vector3 drawPos = thing.DrawPos;
							drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
							Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DesignatorUtility.DragHighlightThingMat, 0);
							seenThings.Add(thing);
						}
					}
				}
				return false;
			}
		}
	}
}
