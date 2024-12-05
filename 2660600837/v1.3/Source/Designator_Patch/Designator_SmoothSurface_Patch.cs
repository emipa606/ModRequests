using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public sealed class Designator_SmoothSurface_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_SmoothSurface_Patch instance = new Designator_SmoothSurface_Patch();

		public static Designator_SmoothSurface_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_SmoothSurface_Patch()
		{
		}

		private enum SmoothSurfaceMode
		{
			All,
			Floor,
			Wall,
		}
		private static SmoothSurfaceMode smoothSurfaceMode = SmoothSurfaceMode.All;

		public override void ClearMode()
		{
			smoothSurfaceMode = SmoothSurfaceMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (smoothSurfaceMode == SmoothSurfaceMode.Floor)
			{
				return "FilterableDesignator.SmoothSurface.Label.Floor".Translate(instance.defaultLabel);
			}
			else if (smoothSurfaceMode == SmoothSurfaceMode.Wall)
			{
				return "FilterableDesignator.SmoothSurface.Label.Wall".Translate(instance.defaultLabel);
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_SmoothSurface), nameof(Designator_SmoothSurface.CanDesignateCell))]
		public class CanDesignateCell_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, IntVec3 c)
			{
				if (!__result.Accepted)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (smoothSurfaceMode == SmoothSurfaceMode.Floor)
				{
					if (c.Filled(Find.CurrentMap))
					{
						__result = "FilterableDesignator.SmoothSurface.Message.MustDesignateFloor".Translate();
					}
				}
				else if (smoothSurfaceMode == SmoothSurfaceMode.Wall)
				{
					if (!c.Filled(Find.CurrentMap))
					{
						__result = "FilterableDesignator.SmoothSurface.Message.MustDesignateWall".Translate();
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
			ThingDef smoothFloorDef = new()
			{
				uiIcon = ContentFinder<Texture2D>.Get("Terrain/Surfaces/SmoothStone"),
				uiIconColor = TerrainDefOf.Sandstone_Smooth.color,
				graphic = TerrainDefOf.Sandstone_Smooth.graphic,
				graphicData = TerrainDefOf.Sandstone_Smooth.graphic.data,
			};
			ThingDef smoothedWallDef = (ThingDef)GenDefDatabase.GetDef(typeof(ThingDef), BackCompatibility.BackCompatibleDefName(typeof(ThingDef), "SmoothedSandstone"));
			var floatMenuList = new List<(SmoothSurfaceMode, ThingDef, string, Texture2D)>
			{
				(SmoothSurfaceMode.All, null, "FilterableDesignator.SmoothSurface.FloatMenuOption.All".Translate().ToString(), instance.icon),
				(SmoothSurfaceMode.Floor, smoothFloorDef, "FilterableDesignator.SmoothSurface.FloatMenuOption.Floor".Translate().ToString(), null),
				(SmoothSurfaceMode.Wall, smoothedWallDef, "FilterableDesignator.SmoothSurface.FloatMenuOption.Wall".Translate().ToString(), Utility.GetLinkedIcon(smoothedWallDef)),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				smoothSurfaceMode = mode;
			});
		}
	}
}