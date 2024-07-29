using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;

namespace ResourceMuffalos
{
    [StaticConstructorOnStartup]
    public static class ResourceMuffalos
    {
        static ResourceMuffalos()
        {
            ResourceMuffalos_Settings.ApplySettings();
        }
    }

	public class ResourceMuffalos_Mod : Mod
	{
		public ResourceMuffalos_Mod(ModContentPack content) : base(content)
		{
			ResourceMuffalos_Mod.settings = base.GetSettings<ResourceMuffalos_Settings>();
		}
		public override string SettingsCategory()
		{
			return "ResourceMuffalos";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			ResourceMuffalos_Settings.DoWindowContents(inRect);
		}

        public override void WriteSettings()
        {
            base.WriteSettings();
            ResourceMuffalos_Settings.ApplySettings();
        }

        public static ResourceMuffalos_Settings settings;
	}

	public class ResourceMuffalos_Settings : ModSettings
    {
		public static int SilverMuffalowoolAmount = 200;
		public static int GoldMuffalowoolAmount = 25;
		public static int JadeMuffalowoolAmount = 50;
		public static int SteelMuffalowoolAmount = 100;
		public static int UraniumMuffalowoolAmount = 25;
		public static int PlasteelMuffalowoolAmount = 25;
		public static int GraniteMuffalowoolAmount = 100;
		public static int MarbleMuffalowoolAmount = 100;
		public static int SandstoneMuffalowoolAmount = 100;
		public static int LimestoneMuffalowoolAmount = 100;
		public static int SlateMuffalowoolAmount = 100;

		public static int SilverMuffaloshearInterval = 25;
		public static int GoldMuffaloshearInterval = 25;
		public static int JadeMuffaloshearInterval = 25;
		public static int SteelMuffaloshearInterval = 25;
		public static int UraniumMuffaloshearInterval = 25;
		public static int PlasteelMuffaloshearInterval = 25;
		public static int GraniteMuffaloshearInterval = 25;
		public static int MarbleMuffaloshearInterval = 25;
		public static int SandstoneMuffaloshearInterval = 25;
		public static int LimestoneMuffaloshearInterval = 25;
		public static int SlateMuffaloshearInterval = 25;


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref SilverMuffalowoolAmount, "SilverMuffalowoolAmount", 200);
			Scribe_Values.Look<int>(ref GoldMuffalowoolAmount, "GoldMuffalowoolAmount", 25);
			Scribe_Values.Look<int>(ref JadeMuffalowoolAmount, "JadeMuffalowoolAmount", 50);
			Scribe_Values.Look<int>(ref SteelMuffalowoolAmount, "SteelMuffalowoolAmount", 100);
			Scribe_Values.Look<int>(ref UraniumMuffalowoolAmount, "UraniumMuffalowoolAmount", 25);
			Scribe_Values.Look<int>(ref PlasteelMuffalowoolAmount, "SilverMuffalowoolAmount", 25);
			Scribe_Values.Look<int>(ref GraniteMuffalowoolAmount, "SilverMuffalowoolAmount", 100);
			Scribe_Values.Look<int>(ref MarbleMuffalowoolAmount, "SilverMuffalowoolAmount", 100);
			Scribe_Values.Look<int>(ref SandstoneMuffalowoolAmount, "SilverMuffalowoolAmount", 100);
			Scribe_Values.Look<int>(ref LimestoneMuffalowoolAmount, "SilverMuffalowoolAmount", 100);
			Scribe_Values.Look<int>(ref SlateMuffalowoolAmount, "SilverMuffalowoolAmount", 100);

			Scribe_Values.Look<int>(ref SilverMuffaloshearInterval, "SilverMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref GoldMuffaloshearInterval, "GoldMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref JadeMuffaloshearInterval, "JadeMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref SteelMuffaloshearInterval, "SteelMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref UraniumMuffaloshearInterval, "UraniumMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref PlasteelMuffaloshearInterval, "PlasteelMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref GraniteMuffaloshearInterval, "GraniteMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref MarbleMuffaloshearInterval, "MarbleMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref SandstoneMuffaloshearInterval, "SandstoneMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref LimestoneMuffaloshearInterval, "LimestoneMuffaloshearInterval", 25);
			Scribe_Values.Look<int>(ref SlateMuffaloshearInterval, "SlateMuffaloshearInterval", 25);

		}
		public static void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			Rect rect = new Rect(0f, 0f, inRect.width - 30f, 800f);
			Rect rect90 = inRect.TopPart(0.85f);
			Widgets.BeginScrollView(rect90, ref ResourceMuffalos_Settings.scrollPosition, rect, true);
			listing_Standard.Begin(rect);
			listing_Standard.Label("Silver Muffalo: " + SilverMuffalowoolAmount.ToString() + " Resources per " + SilverMuffaloshearInterval.ToString() + " Days", -1f, null);
			SilverMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(SilverMuffalowoolAmount, 1f, 250f));
			SilverMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(SilverMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Gold Muffalo: " + GoldMuffalowoolAmount.ToString() + " Resources per " + GoldMuffaloshearInterval.ToString() + " Days", -1f, null);
			GoldMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(GoldMuffalowoolAmount, 1f, 250f));
			GoldMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(GoldMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Jade Muffalo: " + JadeMuffalowoolAmount.ToString() + " Resources per " + JadeMuffaloshearInterval.ToString() + " Days", -1f, null);
			JadeMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(JadeMuffalowoolAmount, 1f, 250f));
			JadeMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(JadeMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Steel Muffalo: " + SteelMuffalowoolAmount.ToString() + " Resources per " + SteelMuffaloshearInterval.ToString() + " Days", -1f, null);
			SteelMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(SteelMuffalowoolAmount, 1f, 250f));
			SteelMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(SteelMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Uranium Muffalo: " + UraniumMuffalowoolAmount.ToString() + " Resources per " + UraniumMuffaloshearInterval.ToString() + " Days", -1f, null);
			UraniumMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(UraniumMuffalowoolAmount, 1f, 250f));
			UraniumMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(UraniumMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Plasteel Muffalo: " + PlasteelMuffalowoolAmount.ToString() + " Resources per " + PlasteelMuffaloshearInterval.ToString() + " Days", -1f, null);
			PlasteelMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(PlasteelMuffalowoolAmount, 1f, 250f));
			PlasteelMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(PlasteelMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Granite Muffalo: " + GraniteMuffalowoolAmount.ToString() + " Resources per " + GraniteMuffaloshearInterval.ToString() + " Days", -1f, null);
			GraniteMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(GraniteMuffalowoolAmount, 1f, 250f));
			GraniteMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(GraniteMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Marble Muffalo: " + MarbleMuffalowoolAmount.ToString() + " Resources per " + MarbleMuffaloshearInterval.ToString() + " Days", -1f, null);
			MarbleMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(MarbleMuffalowoolAmount, 1f, 250f));
			MarbleMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(MarbleMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Sandstone Muffalo: " + SandstoneMuffalowoolAmount.ToString() + " Resources per " + SandstoneMuffaloshearInterval.ToString() + " Days", -1f, null);
			SandstoneMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(SandstoneMuffalowoolAmount, 1f, 250f));
			SandstoneMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(SandstoneMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Limestone Muffalo: " + LimestoneMuffalowoolAmount.ToString() + " Resources per " + LimestoneMuffaloshearInterval.ToString() + " Days", -1f, null);
			LimestoneMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(LimestoneMuffalowoolAmount, 1f, 250f));
			LimestoneMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(LimestoneMuffaloshearInterval, 1f, 50f));
			listing_Standard.Label("Slate Muffalo: " + SlateMuffalowoolAmount.ToString() + " Resources per " + SlateMuffaloshearInterval.ToString() + " Days", -1f, null);
			SlateMuffalowoolAmount = Mathf.RoundToInt(listing_Standard.Slider(SlateMuffalowoolAmount, 1f, 250f));
			SlateMuffaloshearInterval = Mathf.RoundToInt(listing_Standard.Slider(SlateMuffaloshearInterval, 1f, 50f));
			listing_Standard.End();
			Widgets.EndScrollView();
		}
		public static void ApplySettings()
		{
			ThingDefOfMuffalo.SilverMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = SilverMuffalowoolAmount;
			ThingDefOfMuffalo.GoldMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = GoldMuffalowoolAmount;
			ThingDefOfMuffalo.JadeMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = JadeMuffalowoolAmount;
			ThingDefOfMuffalo.SteelMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = SteelMuffalowoolAmount;
			ThingDefOfMuffalo.UraniumMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = UraniumMuffalowoolAmount;
			ThingDefOfMuffalo.PlasteelMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = PlasteelMuffalowoolAmount;
			ThingDefOfMuffalo.GraniteMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = GraniteMuffalowoolAmount;
			ThingDefOfMuffalo.MarbleMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = MarbleMuffalowoolAmount;
			ThingDefOfMuffalo.SandstoneMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = SandstoneMuffalowoolAmount;
			ThingDefOfMuffalo.LimestoneMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = LimestoneMuffalowoolAmount;
			ThingDefOfMuffalo.SlateMuffalo.GetCompProperties<CompProperties_Shearable>().woolAmount = SlateMuffalowoolAmount;

			ThingDefOfMuffalo.SilverMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = SilverMuffaloshearInterval;
			ThingDefOfMuffalo.GoldMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = GoldMuffaloshearInterval;
			ThingDefOfMuffalo.JadeMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = JadeMuffaloshearInterval;
			ThingDefOfMuffalo.SteelMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = SteelMuffaloshearInterval;
			ThingDefOfMuffalo.UraniumMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = UraniumMuffaloshearInterval;
			ThingDefOfMuffalo.PlasteelMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = PlasteelMuffaloshearInterval;
			ThingDefOfMuffalo.GraniteMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = GraniteMuffaloshearInterval;
			ThingDefOfMuffalo.MarbleMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = MarbleMuffaloshearInterval;
			ThingDefOfMuffalo.SandstoneMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = SandstoneMuffaloshearInterval;
			ThingDefOfMuffalo.LimestoneMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = LimestoneMuffaloshearInterval;
			ThingDefOfMuffalo.SlateMuffalo.GetCompProperties<CompProperties_Shearable>().shearIntervalDays = SlateMuffaloshearInterval;
		}
		private static Vector2 scrollPosition = Vector2.zero;
	}

	[DefOf]
	public static class ThingDefOfMuffalo
	{
		public static ThingDef GoldMuffalo;
		public static ThingDef GraniteMuffalo;
		public static ThingDef JadeMuffalo;
		public static ThingDef MarbleMuffalo;
		public static ThingDef PlasteelMuffalo;
		public static ThingDef SandstoneMuffalo;
		public static ThingDef LimestoneMuffalo;
		public static ThingDef SilverMuffalo;
		public static ThingDef SlateMuffalo;
		public static ThingDef SteelMuffalo;
		public static ThingDef UraniumMuffalo;
	}
}
