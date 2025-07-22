using RimWorld;
using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AnimalDiscovery
{
	[StaticConstructorOnStartup]
    internal static class AnimalDiscoveryUtil
    {
        public static bool RadioButtonLabeledTextPos(Rect rect, string labelText, bool chosen, TextAnchor textAnchor = TextAnchor.MiddleLeft)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = textAnchor;
			Widgets.Label(rect, labelText);
			Text.Anchor = anchor;
			bool flag = Widgets.ButtonInvisible(rect, true);
			if (flag && !chosen)
			{
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			}
			RadioButtonDraw(rect.x + rect.width - 24f, rect.y + rect.height / 2f - 12f, chosen);
			return flag;
		}

		private static void RadioButtonDraw(float x, float y, bool chosen)
		{
			Color color = GUI.color;
			GUI.color = Color.white;
			Texture2D image;
			if (chosen)
			{
				image = Widgets.RadioButOnTex;
			}
			else
			{
				image = RadioButOffTex;
			}
			GUI.DrawTexture(new Rect(x, y, 24f, 24f), image);
			GUI.color = color;
		}

		public static bool ButtonTextLabeledPct(Rect rect, string label, string buttonLabel, float labelPct, TextAnchor anchor = TextAnchor.UpperLeft, string highlightTag = null, string tooltip = null, Texture2D labelIcon = null)
        {
            float height = Math.Max(Text.CalcHeight(label, rect.width * labelPct), 30f);
            //Rect rect = base.GetRect(height, 1f);
            Rect rect2 = rect.RightPart(1f - labelPct);
            rect2.height = 30f;
            if (highlightTag != null)
            {
                UIHighlighter.HighlightOpportunity(rect, highlightTag);
            }
            bool result = false;
            Rect rect3 = rect.LeftPart(labelPct);
            Text.Anchor = anchor;
            Widgets.Label(rect3, label);
            result = Widgets.ButtonText(rect2, buttonLabel.Truncate(rect2.width - 20f, null), true, true, true, null);
            Text.Anchor = TextAnchor.UpperLeft;
            if (labelIcon != null)
            {
                GUI.DrawTexture(new Rect(Text.CalcSize(label).x + 10f, rect3.y + (rect3.height - Text.LineHeight) / 2f, Text.LineHeight, Text.LineHeight), labelIcon);
            }
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect3))
                {
                    Widgets.DrawHighlight(rect3);
                }
                TooltipHandler.TipRegion(rect3, tooltip);
            }
            //base.Gap(this.verticalSpacing);
            return result;
        }

        private static readonly Texture2D RadioButOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff", true);
	}
}
