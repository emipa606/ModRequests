using HarmonyLib;
using Verse;
using System;
using UnityEngine;
using static Moonlight.ModSettings_Moonlight;
using static Moonlight.MoonlightUtility;

namespace Moonlight
{
	public class Weather : DefModExtension {}
	
	public class MoonlightMod : Mod
	{	
		public MoonlightMod(ModContentPack content) : base(content)
		{
			base.GetSettings<ModSettings_Moonlight>();
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
		}

		 public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard options = new Listing_Standard();
			options.Begin(inRect);
			
			options.Label("Moonlight_DarknessSlider".Translate("0.35","0.25","0.5") + Math.Round(darkest, 2), -1f, tooltip:null);
			darkest = options.Slider(darkest, 0.25f, 0.5f);
			
			options.Label("Moonlight_BrightnessSlider".Translate("1","0.5","1") + Math.Round(brightest, 2), -1f, tooltip:null);
			brightest = options.Slider(brightest, 0.5f, 1f);
			
			if (Prefs.DevMode) options.CheckboxLabeled("DevMode: Enable logging", ref logging, null);
			options.End();
		}
		public override string SettingsCategory()
		{
			return "Moonlight";
		}
		public override void WriteSettings()
		{
			base.WriteSettings();
			RecalculateArray();
		}
    }

	public class ModSettings_Moonlight : ModSettings
	{
		public override void ExposeData()
		{
			Scribe_Values.Look(ref darkest, "darkest", 0.35f);
            Scribe_Values.Look(ref brightest, "brightest", 1f);
			base.ExposeData();
		}

		public static float darkest = 0.35f, brightest = 1f;
		public static bool logging;
	}
}
