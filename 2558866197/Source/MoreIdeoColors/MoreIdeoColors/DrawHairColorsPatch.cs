using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace MoreIdeoColors
{
	[HarmonyPatch(typeof(Dialog_StylingStation))]
	[HarmonyPatch("DrawHairColors")]
	class DrawHairColorsPatch
	{
		/*private static bool Prefix(Dialog_StylingStation __instance, Pawn ___pawn, ref Color ___desiredHairColor, Rect rect)
		{
			rect.height = 400f;
			float num = rect.y;
			PropertyInfo AllHairColorsProp = __instance.GetType().GetProperty("AllHairColors",
				BindingFlags.NonPublic | BindingFlags.Instance);

			List<Color> AllHairColors = (List<Color>)AllHairColorsProp.GetValue(__instance);
			float colorsHeight;
			ColorSelectorClass.ColorSelector(new Rect(rect.x, num, rect.width, 284f), ref ___desiredHairColor, AllHairColors, out colorsHeight, null, 22, 2);
			//__instance.GetType().GetProperty("colorsHeight") = colorsHeight;

			//ColorSelectorClass.ColorSelector(new Rect(rect.x, num, rect.width, 184f), ref ___desiredHairColor, AllHairColors, null, 22, 2);
			num -= 45f;
			if (___desiredHairColor != ___pawn.story.HairColor && ___desiredHairColor != ___pawn.style.nextHairColor)
			{
				Widgets.Label(new Rect(rect.x, num, rect.width, Text.LineHeight), "Required".Translate() + ": " + 1 + " " + ThingDefOf.Dye.LabelCap);
				num += Text.LineHeight;
				if (___pawn.Map.resourceCounter.GetCount(ThingDefOf.Dye) < 1)
				{
					Rect rect2 = new Rect(rect.x, num, rect.width, Text.LineHeight);
					Color color = GUI.color;
					GUI.color = ColorLibrary.RedReadable;
					Widgets.Label(rect2, "NotEnoughDye".Translate() + " " + "NotEnoughDyeWillRecolorHair".Translate());
					GUI.color = color;
					num += rect2.height;
				}
			}
			return false;
		}*/
		/*private static bool Prefix(Dialog_StylingStation __instance, float ___viewRectHeight, Pawn ___pawn, bool ___devEditMode, Rect rect, ref Vector2 scrollPosition, Action<Rect, T> drawAction, Action<T> selectAction, Func<StyleItemDef, bool> hasStyleItem, Func<StyleItemDef, bool> hadStyleItem, Func<StyleItemDef, bool> extraValidator = null, bool doColors = false) where T : StyleItemDef
		{
			Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, ___viewRectHeight);
			int num = Mathf.FloorToInt(viewRect.width / 60f) - 1;
			float num2 = (viewRect.width - (float)num * 60f - (float)(num - 1) * 10f) / 2f;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			PropertyInfo tmpStyleItemsProp = typeof(Dialog_StylingStation).GetProperty("tmpStyleItems",
				BindingFlags.NonPublic | BindingFlags.Static);

			List<StyleItemDef> tmpStyleItems = (List<StyleItemDef>)tmpStyleItemsProp.GetValue(null);
			tmpStyleItems.Clear();
			tmpStyleItems.AddRange(from x in DefDatabase<T>.AllDefs
														 where (___devEditMode || PawnStyleItemChooser.WantsToUseStyle(___pawn, x, null) || hadStyleItem(x)) && (extraValidator == null || extraValidator(x))
														 select x);
			tmpStyleItems.SortBy((StyleItemDef x) => -PawnStyleItemChooser.StyleItemChoiceLikelihoodFor(x, ___pawn));
			if (tmpStyleItems.NullOrEmpty<StyleItemDef>())
			{
				Widgets.NoneLabelCenteredVertically(rect, "(" + "NoneUsableForPawn".Translate(___pawn.Named("PAWN")) + ")");
			}
			else
			{
				Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
				foreach (StyleItemDef styleItemDef in tmpStyleItems)
				{
					if (num5 >= num - 1)
					{
						num5 = 0;
						num4++;
					}
					else if (num3 > 0)
					{
						num5++;
					}
					Rect rect2 = new Rect(rect.x + num2 + (float)num5 * 60f + (float)num5 * 10f, rect.y + (float)num4 * 60f + (float)num4 * 10f, 60f, 60f);
					Widgets.DrawHighlight(rect2);
					if (Mouse.IsOver(rect2))
					{
						Widgets.DrawHighlight(rect2);
						TooltipHandler.TipRegion(rect2, styleItemDef.LabelCap);
					}
					if (drawAction != null)
					{
						drawAction(rect2, styleItemDef as T);
					}
					if (hasStyleItem(styleItemDef))
					{
						Widgets.DrawBox(rect2, 2, null);
					}
					if (Widgets.ButtonInvisible(rect2, true))
					{
						if (selectAction != null)
						{
							selectAction(styleItemDef as T);
						}
						SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
						___pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
					}
					num3++;
				}
				if (Event.current.type == EventType.Layout)
				{
					___viewRectHeight = (float)(num4 + 1) * 60f + (float)num4 * 10f + 10f;
				}
				Widgets.EndScrollView();
			}
			if (doColors)
			{
				MethodInfo DrawHairColorsMethod = __instance.GetType().GetMethod("DrawHairColors",
				BindingFlags.NonPublic | BindingFlags.Instance);

				DrawHairColorsMethod.Invoke(__instance, new object[] { new Rect(rect.x, rect.yMax + 10f, rect.width, 200f) });
				*//*this.DrawHairColors(new Rect(rect.x, rect.yMax + 10f, rect.width, 200f));*//*
				return false;
			}
			return true;
		}*/
	}
}
