using System;
using System.Reflection;
using Crystosentient.Dictionary;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Crystosentient.Settings
{
	// Token: 0x02000014 RID: 20
	public sealed class MenuUI : Mod
	{
		// Token: 0x0600006E RID: 110 RVA: 0x00002469 File Offset: 0x00000669
		public MenuUI(ModContentPack content) : base(content)
		{
			base.GetSettings<Settings>();
		}

		// Token: 0x0600006F RID: 111 RVA: 0x0000248D File Offset: 0x0000068D
		public override string SettingsCategory()
		{
			return Static.LabelModName;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000047B8 File Offset: 0x000029B8
		public override void DoSettingsWindowContents(Rect rect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = rect.width;
			listing_Standard.Begin(rect);
			listing_Standard.Gap(25f);
			Rect rect2 = listing_Standard.GetRect(30f);
			Widgets.Label(rect2, Static.LabelAllowedToSpawn);
			Text.Font = GameFont.Small;
			listing_Standard.Gap(10f);
			Rect rect8 = listing_Standard.GetRect(30f);
			Rect rect9 = rect8.RightHalf().Rounded();
			MenuUI.DualCheckboxesWithIcons_ThingDef(rect8.LeftHalf(), DefOfThing.GDCRYST_BUILDABLE_WallAmethystRough, "Gem Biome", ref Settings.SpawnGemGarden);
			Text.Font = GameFont.Medium;
			listing_Standard.End();
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004888 File Offset: 0x00002A88
		private static void DualCheckboxesWithIcons_ThingDef(Rect rect, ThingDef leftThingDef, string leftLabel, ref bool leftBool)
		{
			Rect rect2 = rect.LeftHalf().RightPartPixels(150f).Rounded();
			Rect rect4 = rect.LeftHalf().LeftHalf().LeftHalf().RightPartPixels(30f).Rounded();
			Widgets.ThingIcon(rect4, leftThingDef, null, null, 1f, null);
			Widgets.CheckboxLabeled(rect2, leftLabel, ref leftBool, false, null, null, false);
			Widgets.DrawHighlightIfMouseover(rect2);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00004954 File Offset: 0x00002B54
		private static void DualCheckboxesWithIcons_Texture2D(Rect rect, Texture2D leftIcon, Texture2D rightIcon, string leftLabel, string rightLabel, ref bool leftBool, ref bool rightBool)
		{
			Rect rect2 = rect.LeftHalf().RightPartPixels(150f).Rounded();
			Rect rect3 = rect.RightHalf().RightPartPixels(150f).Rounded();
			Rect rect4 = rect.LeftHalf().LeftHalf().LeftHalf().RightPartPixels(30f).Rounded();
			Rect rect5 = rect.RightHalf().LeftHalf().LeftHalf().RightPartPixels(30f).Rounded();
			GUI.DrawTexture(rect4, leftIcon);
			GUI.DrawTexture(rect5, rightIcon);
			Widgets.CheckboxLabeled(rect2, leftLabel, ref leftBool, false, null, null, false);
			Widgets.CheckboxLabeled(rect3, rightLabel, ref rightBool, false, null, null, false);
			Widgets.DrawHighlightIfMouseover(rect2);
			Widgets.DrawHighlightIfMouseover(rect3);
		}
	}
}
