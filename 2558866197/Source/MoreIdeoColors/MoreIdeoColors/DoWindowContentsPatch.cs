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
    [HarmonyPatch("DoWindowContents")]
    class DoWindowContentsPatch
    {
        static bool Prefix(Dialog_StylingStation __instance, ref bool ___devEditMode, Pawn ___pawn, Rect inRect)
        {
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(inRect)
			{
				height = Text.LineHeight * 2f
			};
			Widgets.Label(rect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(___pawn.Name.ToStringShort, ___pawn.gender, false, true).ApplyTag(TagType.Name, null));
			Text.Font = GameFont.Small;
			inRect.yMin = rect.yMax + 4f;
			Rect rect2 = inRect;
			rect2.width *= 0.2f;
			rect2.yMax -= 40f + 4f;
			MethodInfo DrawPawnMethod = __instance.GetType().GetMethod("DrawPawn",
					BindingFlags.NonPublic | BindingFlags.Instance);

			DrawPawnMethod.Invoke(__instance, new object[] { rect2 });
			/*this.DrawPawn(rect2);*/
			Rect rect3 = inRect;
			rect3.xMin = rect2.xMax + 10f;
			rect3.yMax -= 40f + 4f;
			MethodInfo DrawTabsMethod = __instance.GetType().GetMethod("DrawTabs",
					BindingFlags.NonPublic | BindingFlags.Instance);

			DrawTabsMethod.Invoke(__instance, new object[] { rect3 });
			MethodInfo DrawBottomButtonsMethod = __instance.GetType().GetMethod("DrawBottomButtons",
					BindingFlags.NonPublic | BindingFlags.Instance);

			DrawBottomButtonsMethod.Invoke(__instance, new object[] { inRect });
			/*this.DrawTabs(rect3);
			this.DrawBottomButtons(inRect);*/
			if (Prefs.DevMode)
			{
				Widgets.CheckboxLabeled(new Rect(inRect.xMax - 120f, 0f, 120f, 30f), "Dev: Show all", ref ___devEditMode, false, null, null, false);
			}
			return false;
        }
    }
}
