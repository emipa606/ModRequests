using System;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public class Settings : ModSettings
	{
		public static bool IsEnabled = true;

		public static Vector2 scrollPosition = Vector2.zero;

		override public void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref IsEnabled, "IsEnabled");
		}

		public static void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new Listing_Standard();

			const int EnabledHeight = 32;

			listing.Begin(new Rect(inRect.x, inRect.y, inRect.width * .4f, EnabledHeight));
			listing.CheckboxLabeled("Enabled".Translate(), ref IsEnabled);
			listing.End();
		}
	}
}
