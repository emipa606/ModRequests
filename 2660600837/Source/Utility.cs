using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public static class Utility
	{
		public const int SameKind = 0x100;
		public const int SameStuff = 0x200;
		public const int SameKindAndStuff = 0x300;

		public static void BuildFloatMenuOption<T>(Designator instance, Event ev, List<(T mode, ThingDef thingDef, string text, Texture icon)> floatMenuList, Action<T, ThingDef> setMode)
		{
			if (!ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			List<FloatMenuOption> list = new();
			foreach (var floatMenuRow in floatMenuList)
			{
				string text = floatMenuRow.text;
				ThingDef iconThingDef = floatMenuRow.thingDef;
				Texture icon = floatMenuRow.icon ?? iconThingDef?.uiIcon;
				Color iconColor = iconThingDef?.graphicData?.colorTwo ?? Color.white;
				if (iconColor == Color.white)
				{
					iconColor = iconThingDef?.uiIconColor ?? Color.white;
				}
				FloatMenuOption floatMenuOption = new(text, delegate
				{
					ReversePatches.ProcessInput(instance, ev);
					Find.DesignatorManager.Select(instance);
					setMode(floatMenuRow.mode, floatMenuRow.thingDef);
				}, (Texture2D)icon, iconColor);
//				floatMenuOption.tutorTag = "SelectStuff-" + thingDef.defName + "-" + localStuffDef.defName;
				list.Add(floatMenuOption);
			}

			FloatMenu floatMenu = new(list) { vanishIfMouseDistant = true };
//			floatMenu.onCloseCallback = delegate
//			{
//				writeStuff = true;
//			};
			Find.WindowStack.Add(floatMenu);
			Find.DesignatorManager.Select(instance);
		}

		public static Texture2D GetLinkedIcon(ThingDef thingDef)
		{
			Texture2D icon = thingDef.uiIcon;
			if (!icon.isReadable)
			{
				icon = TextureAtlasHelper.MakeReadableTextureInstance(icon);
			}
			Material iconMat = thingDef.graphic.MatSingle;
			Vector2 iconSize = new Vector2(icon.width, icon.height) * iconMat.mainTextureScale;
			Vector2 mergin = new Vector2(icon.width, icon.height) / 4 - iconSize;
			var pixels = icon.GetPixels((int)mergin.x / 2, (int)mergin.y / 2 + icon.height / 2, (int)iconSize.x, (int)iconSize.y);
			icon = new Texture2D((int)iconSize.x, (int)iconSize.y);
			icon.SetPixels(pixels);
			icon.Apply();
			return icon;
		}

		public static void ClearAllMode()
		{
			Designator_Cancel_Patch.Instance.ClearMode();
			Designator_Deconstruct_Patch.Instance.ClearMode();
			Designator_ExtractTree_Patch.Instance.ClearMode();
			Designator_Mine_Patch.Instance.ClearMode();
			Designator_SmoothSurface_Patch.Instance.ClearMode();
			Designator_Haul_Patch.Instance.ClearMode();
			Designator_PlantsCut_Patch.Instance.ClearMode();
			Designator_PlantsHarvest_Patch.Instance.ClearMode();
			Designator_PlantsHarvestWood_Patch.Instance.ClearMode();
			Designator_Tame_Patch.Instance.ClearMode();
			Designator_Forbid_Patch.Instance.ClearMode();
			Designator_Unforbid_Patch.Instance.ClearMode();
		}

		[HarmonyPatch]
		public static class ReversePatches
		{
			[HarmonyReversePatch]
			[HarmonyPatch(typeof(Command), nameof(Command.ProcessInput))]
			public static void ProcessInput(object instance, Event ev)
			{
				// its a stub so it has no initial content
				throw new NotImplementedException("It's a stub");
			}

			[HarmonyReversePatch]
			[HarmonyPatch(typeof(Designator), "CheckCanInteract")]
			public static bool CheckCanInteract(object instance)
			{
				// its a stub so it has no initial content
				throw new NotImplementedException("It's a stub");
			}
		}
	}
}
