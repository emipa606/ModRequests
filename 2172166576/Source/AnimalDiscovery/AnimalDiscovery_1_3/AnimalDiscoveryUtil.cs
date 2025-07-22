using RimWorld;
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

		private static readonly Texture2D RadioButOffTex = ContentFinder<Texture2D>.Get("UI/Widgets/RadioButOff", true);
	}
}
