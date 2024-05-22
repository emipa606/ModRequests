using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Steam;


namespace dragonagemetals
{
	public class DragonAgeMetalsSettings : ModSettings
	{
		public void DoWindowContents(Rect canvas)
		{
			if (Widgets.ButtonText(new Rect(canvas.width - 170f, 0f, 140f, 28f), "Reset to defaults".Flatten(), true, true, true, null))
			{
				this.veridiumCommonality = 0.9f;
				this.veridiumMineableCommonality = 0.5f;
				this.veridiumDeepCommonality = 4f;

				this.SilveriteOreCommonality = 0.3f;
				this.SilveriteMineableCommonality = 0.3f;
				this.SilveriteOreDeepCommonality = 2f;

				this.volcanicAurumCommonality = 0.2f;
				this.volcanicAurumMineableCommonality = 0.2f;
				this.volcanicAurumDeepCommonality = 1f;

				this.lyriumCommonality = 0.1f;
				this.lyriumMineableCommonality = 0.1f;
				this.lyriumDeepCommonality = 4f;

				this.meteoriteOreCommonality = 0f;

				this.foldedLyriumCommonality = 0.05f;
			}
			Rect outRect = canvas.ContractedBy(8f);
			Rect rect = new Rect(0f, 0f, outRect.width - 40f, 1200f);
			Widgets.BeginScrollView(outRect, ref DragonAgeMetalsSettings.scrollPosition, rect, true);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = rect.width;
			listing_Standard.Begin(rect);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Restart needed for changes to take effect".Flatten(), -1f, null);
			listing_Standard.Gap(24f);
			Text.Font = GameFont.Small;
			listing_Standard.Label("Veridium".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.veridiumCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.9)", -1f, null);
			this.veridiumCommonality = listing_Standard.Slider(this.veridiumCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("MineableCommonality".Flatten() + ":   " + this.veridiumMineableCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.5)", -1f, null);
			this.veridiumMineableCommonality = listing_Standard.Slider(this.veridiumMineableCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("DeepCommonality".Flatten() + ":   " + this.veridiumDeepCommonality.ToString("0") + "  (" + "Default".Flatten() + ": 4)", -1f, null);
			this.veridiumDeepCommonality = listing_Standard.Slider(this.veridiumDeepCommonality, 0f, 50f);
			listing_Standard.Gap(24f);

			Text.Font = GameFont.Small;
			listing_Standard.Label("SilveriteOre".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.SilveriteOreCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.3)", -1f, null);
			this.SilveriteOreCommonality = listing_Standard.Slider(this.SilveriteOreCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("MineableCommonality".Flatten() + ":   " + this.SilveriteMineableCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.3)", -1f, null);
			this.SilveriteMineableCommonality = listing_Standard.Slider(this.SilveriteMineableCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("DeepCommonality".Flatten() + ":   " + this.SilveriteOreDeepCommonality.ToString("0") + "  (" + "Default".Flatten() + ": 2)", -1f, null);
			this.SilveriteOreDeepCommonality = listing_Standard.Slider(this.SilveriteOreDeepCommonality, 0f, 50f);
			listing_Standard.Gap(24f);

			Text.Font = GameFont.Small;
			listing_Standard.Label("Volcanic Aurum".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.volcanicAurumCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.2)", -1f, null);
			this.volcanicAurumCommonality = listing_Standard.Slider(this.volcanicAurumCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("MineableCommonality".Flatten() + ":   " + this.volcanicAurumMineableCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.2)", -1f, null);
			this.volcanicAurumMineableCommonality = listing_Standard.Slider(this.volcanicAurumMineableCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("DeepCommonality".Flatten() + ":   " + this.volcanicAurumDeepCommonality.ToString("0") + "  (" + "Default".Flatten() + ": 1)", -1f, null);
			this.volcanicAurumDeepCommonality = listing_Standard.Slider(this.volcanicAurumDeepCommonality, 0f, 50f);
			listing_Standard.Gap(24f);

			Text.Font = GameFont.Small;
			listing_Standard.Label("Lyrium".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.lyriumCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.1)", -1f, null);
			this.lyriumCommonality = listing_Standard.Slider(this.lyriumCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("MineableCommonality".Flatten() + ":   " + this.lyriumMineableCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.1)", -1f, null);
			this.lyriumMineableCommonality = listing_Standard.Slider(this.lyriumMineableCommonality, 0f, 10f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("DeepCommonality".Flatten() + ":   " + this.lyriumDeepCommonality.ToString("0") + "  (" + "Default".Flatten() + ": 4)", -1f, null);
			this.lyriumDeepCommonality = listing_Standard.Slider(this.lyriumDeepCommonality, 0f, 50f);
			listing_Standard.Gap(24f);

			Text.Font = GameFont.Small;
			listing_Standard.Label("Meteorite Ore".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.meteoriteOreCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0)", -1f, null);
			this.meteoriteOreCommonality = listing_Standard.Slider(this.meteoriteOreCommonality, 0f, 10f);
			listing_Standard.Gap(24f);

			Text.Font = GameFont.Small;
			listing_Standard.Label("Folded Lyrium".Flatten(), -1f, null);
			listing_Standard.GapLine(12f);
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("Commonality".Flatten() + ":   " + this.foldedLyriumCommonality.ToString("0.0") + "  (" + "Default".Flatten() + ": 0.05)", -1f, null);
			this.foldedLyriumCommonality = listing_Standard.Slider(this.foldedLyriumCommonality, 0f, 10f);
			listing_Standard.Gap(24f);
			listing_Standard.End();
			Widgets.EndScrollView();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.veridiumCommonality, "veridiumCommonality", 0.8f, false);
			Scribe_Values.Look<float>(ref this.veridiumMineableCommonality, "veridiumMineableCommonality", 1f, false);
			Scribe_Values.Look<float>(ref this.veridiumDeepCommonality, "veridiumDeepCommonality", 4f, false);

			Scribe_Values.Look<float>(ref this.SilveriteOreCommonality, "SilveriteOreCommonality", 0.3f, false);
			Scribe_Values.Look<float>(ref this.SilveriteMineableCommonality, "SilveriteMineableCommonality", 0.3f, false);
			Scribe_Values.Look<float>(ref this.SilveriteOreDeepCommonality, "SilveriteOreDeepCommonality", 2f, false);

			Scribe_Values.Look<float>(ref this.volcanicAurumCommonality, "volcanicAurumCommonality", 0.2f, false);
			Scribe_Values.Look<float>(ref this.volcanicAurumMineableCommonality, "volcanicAurumMineableCommonality", 0.2f, false);
			Scribe_Values.Look<float>(ref this.volcanicAurumDeepCommonality, "volcanicAurumDeepCommonality", 1f, false);

			Scribe_Values.Look<float>(ref this.lyriumCommonality, "lyriumCommonality", 0.1f, false);
			Scribe_Values.Look<float>(ref this.lyriumMineableCommonality, "lyriumMineableCommonality", 0.1f, false);
			Scribe_Values.Look<float>(ref this.lyriumDeepCommonality, "lyriumDeepCommonality", 4f, false);

			Scribe_Values.Look<float>(ref this.meteoriteOreCommonality, "meteoriteOreCommonality", 0f, false);

			Scribe_Values.Look<float>(ref this.foldedLyriumCommonality, "foldedLyriumCommonality", 0.05f, false);
		}

		public float veridiumCommonality = 0.8f;
		public float veridiumMineableCommonality = 1f;
		public float veridiumDeepCommonality = 4f;

		public float SilveriteOreCommonality = 0.3f;
		public float SilveriteMineableCommonality = 0.3f;
		public float SilveriteOreDeepCommonality = 2f;

		public float volcanicAurumCommonality = 0.2f;
		public float volcanicAurumMineableCommonality = 0.2f;
		public float volcanicAurumDeepCommonality = 1f;

		public float lyriumCommonality = 0.1f;
		public float lyriumMineableCommonality = 0.1f;
		public float lyriumDeepCommonality = 4f;

		public float meteoriteOreCommonality = 0f;

		public float foldedLyriumCommonality = 0.05f;

		private const float ScrollbarWidth = 12f;

		private static Vector2 scrollPosition = Vector2.zero;
	}
}