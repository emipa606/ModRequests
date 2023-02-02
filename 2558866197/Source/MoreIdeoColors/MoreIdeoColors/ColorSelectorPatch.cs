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
    class ColorSelectorPatch
	{

		[HarmonyPatch(typeof(Dialog_StylingStation))]
		[HarmonyPatch("ColorSelector")]
		private static bool Prefix(bool __result, Rect rect, ref Color color, List<Color> colors, Texture icon = null, int colorSize = 22, int colorPadding = 2)
		{
			bool result = false;
			int num = colorSize + colorPadding * 2;
			float num2 = (icon != null) ? ((float)(colorSize * 4) + 10f) : 0f;
			int num3 = Mathf.FloorToInt((rect.width - num2) / (float)(num + colorPadding));
			int num4 = Mathf.CeilToInt((float)colors.Count / (float)num3);
			GUI.BeginGroup(rect);
			if (icon != null)
			{
				GUI.color = color;
				GUI.DrawTexture(new Rect(5f, 5f, (float)(colorSize * 4), (float)(colorSize * 4)), icon);
				GUI.color = Color.white;
			}
			int num5 = 0;
			foreach (Color color2 in colors)
			{
				int num6 = num5 / num3;
				int num7 = num5 % num3;
				/*float num8 = (icon != null) ? ((num2 - (float)(num * num4) - (float)colorPadding) / 2f) : 0f;*/
				float num8 = 0f;
				Rect rect2 = new Rect(num2 + (float)(num7 * num) + (float)(num7 * colorPadding), num8 + (float)(num6 * num) + (float)(num6 * colorPadding), (float)num, (float)num);
				Widgets.DrawLightHighlight(rect2);
				Widgets.DrawHighlightIfMouseover(rect2);
				if (Approximately(color.r, color2.r) && Approximately(color.g, color2.g) && Approximately(color.b, color2.b) && Approximately(color.a, color2.a))
				{
					Widgets.DrawBox(rect2, 1, null);
				}
				Widgets.DrawBoxSolid(new Rect(rect2.x + (float)colorPadding, rect2.y + (float)colorPadding, (float)colorSize, (float)colorSize), color2);
				if (Widgets.ButtonInvisible(rect2, true))
				{
					result = true;
					color = color2;
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				}
				num5++;
			}
			GUI.EndGroup();
			__result = result;
			return false;
		}
		internal static bool Approximately(float f1, float f2)
		{
			return Mathf.Abs(f1 - f2) < 0.01f;
		}
	}

}
