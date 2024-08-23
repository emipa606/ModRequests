using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.Sound;
using System.Reflection;

namespace MoreIdeoColors
{
	[HarmonyPatch(typeof(Dialog_StylingStation))]
	[HarmonyPatch("DrawApparelColor")]
	class DrawApparelColorPatch
    {

		private static bool Prefix(Dialog_StylingStation __instance, ref float ___viewRectHeight, ref Vector2 ___apparelColorScrollPosition, Pawn ___pawn, Dictionary<Apparel, Color> ___apparelColors, Rect rect)
		{
			bool flag = false;
			Rect viewRect = new Rect(rect.x, rect.y, rect.width - 16f, ___viewRectHeight);
			Widgets.BeginScrollView(rect, ref ___apparelColorScrollPosition, viewRect, true);
			int num = 0;
			float num2 = rect.y;
			PropertyInfo AllColorsProp = __instance.GetType().GetProperty("AllColors",
				BindingFlags.NonPublic | BindingFlags.Instance);
				
            List<Color> allColors = (List<Color>)AllColorsProp.GetValue(__instance);
            foreach (Apparel apparel in ___pawn.apparel.WornApparel)
			{
				Rect rect2 = new Rect(rect.x, num2, viewRect.width, 140f);
				Color color = ___apparelColors[apparel];

				float colorHeight;
				flag |= ColorSelectorClass.ColorSelector(rect2, ref color, allColors, out colorHeight, apparel.def.uiIcon, 22, 2);
				num2 += rect2.height + 10f;
				if (___pawn.Ideo != null)
				{
					rect2 = new Rect(rect.x, num2, viewRect.width / 2f - 10f, 24f);
					if (Widgets.ButtonText(rect2, "SetIdeoColor".Translate(), true, true, true))
					{
						flag = true;
						color = ___pawn.Ideo.ApparelColor;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
					}
				}
				Pawn_StoryTracker story = ___pawn.story;
				if (story != null && story.favoriteColor != null)
				{
					rect2 = new Rect(rect2.xMax + 10f, num2, viewRect.width / 2f - 10f, 24f);
					if (Widgets.ButtonText(rect2, "SetFavoriteColor".Translate(), true, true, true))
					{
						flag = true;
						color = ___pawn.story.favoriteColor.Value;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
					}
				}
				if (color != apparel.DrawColor)
				{
					num++;
				}
				___apparelColors[apparel] = color;
				num2 += 34f;
			}
			if (num > 0)
			{
				Widgets.Label(new Rect(rect.x, num2, rect.width, 25f), "Required".Translate() + ": " + num + " " + ThingDefOf.Dye.LabelCap);
				num2 += 25f;
			}
			if (___pawn.Map.resourceCounter.GetCount(ThingDefOf.Dye) < num)
			{
				Rect rect3 = new Rect(rect.x, num2, rect.width - 16f - 10f, 60f);
				Color color2 = GUI.color;
				GUI.color = ColorLibrary.RedReadable;
				Widgets.Label(rect3, "NotEnoughDye".Translate() + " " + "NotEnoughDyeWillRecolorApparel".Translate());
				GUI.color = color2;
				num2 += rect3.height;
			}
			if (Event.current.type == EventType.Layout)
			{
				___viewRectHeight = num2 - rect.y;
			}
			Widgets.EndScrollView();
			return false;
		}
	}
}
