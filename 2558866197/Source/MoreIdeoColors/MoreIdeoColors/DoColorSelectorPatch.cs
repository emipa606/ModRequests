using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse;
using System.Reflection;

namespace MoreIdeoColors
{
    [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols))]
    [HarmonyPatch("DoColorSelector")]
    class DoColorSelectorPatch
    {
        private static bool Prefix(ref ColorDef ___newColorDef, IdeoIconDef ___newIconDef, Rect mainRect, ref float curY)
        {
			int num = 26;
			float num2 = 98f;
			int num3 = Mathf.FloorToInt((mainRect.width - num2) / (float)(num + 2));
			PropertyInfo IdeoColorsSortedProp = typeof(Dialog_ChooseIdeoSymbols).GetProperty("IdeoColorsSorted",
				BindingFlags.NonPublic | BindingFlags.Static);

            List<ColorDef> IdeoColorsSorted = (List<ColorDef>)IdeoColorsSortedProp.GetValue(null);
			int num4 = Mathf.CeilToInt((float)IdeoColorsSorted.Count / (float)num3);
			GUI.BeginGroup(mainRect);
			GUI.color = ___newColorDef.color;
			GUI.DrawTexture(new Rect(5f, 5f, 88f, 88f), ___newIconDef.Icon);
			GUI.color = Color.white;
			curY += num2;
			int num5 = 0;
            /*float num8 = (num2 - (float)(num * num4) - 2f) / 2f;*/

            float num8 = 0f;
            foreach (ColorDef colorDef in IdeoColorsSorted)
			{
				int num6 = num5 / num3;
				int num7 = num5 % num3;
				
				Rect rect = new Rect(num2 + (float)(num7 * num) + (float)(num7 * 2), num8 + (float)(num6 * num) + (float)(num6 * 2), (float)num, (float)num);
				Widgets.DrawLightHighlight(rect);
				Widgets.DrawHighlightIfMouseover(rect);
				if (___newColorDef == colorDef)
				{
					Widgets.DrawBox(rect, 1, null);
				}
				Widgets.DrawBoxSolid(new Rect(rect.x + 2f, rect.y + 2f, 22f, 22f), colorDef.color);
				if (Widgets.ButtonInvisible(rect, true))
				{
					___newColorDef = colorDef;
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
				}
				curY = Mathf.Max(curY, mainRect.yMin + rect.yMax);
				num5++;
			}
			GUI.EndGroup();
			curY += 4f;

			return false;
        }
    }
}
