using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace SaferPasteDispenser
{
	public class Settings : ModSettings
	{
		public enum NumberOfIngredients
		{
			Single = 1,
			Double = 2,
			Triple = 3,
		}
		public static NumberOfIngredients numberOfIngredients = NumberOfIngredients.Single;

		override public void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref numberOfIngredients, "numberOfIngredients");
		}

		public static void DoSettingsWindowContents(Rect inRect)
		{
			var listing = new Listing_Standard();

			listing.Begin(new Rect(inRect.x, inRect.y, inRect.width / 2f, inRect.height));

			listing.Label("SaferPasteDispenser.AllowMultipleFeeds".Translate(ThingDefOf.MealNutrientPaste.label));
			if (listing.RadioButton("　" + "SaferPasteDispenser.Single".Translate() + " " + "SaferPasteDispenser.Default".Translate(), (numberOfIngredients == NumberOfIngredients.Single), tooltip: "SaferPasteDispenser.Single.desc".Translate()))
			{
				numberOfIngredients = NumberOfIngredients.Single;
			}

			if (listing.RadioButton("　" + "SaferPasteDispenser.Double".Translate(), (numberOfIngredients == NumberOfIngredients.Double), tooltip: "SaferPasteDispenser.Double.desc".Translate()))
			{
				numberOfIngredients = NumberOfIngredients.Double;
			}

			if (listing.RadioButton("　" + "SaferPasteDispenser.Triple".Translate(), (numberOfIngredients == NumberOfIngredients.Triple), tooltip: "SaferPasteDispenser.Triple.desc".Translate()))
			{
				numberOfIngredients = NumberOfIngredients.Triple;
			}

			Text.Font = GameFont.Tiny;
			listing.Label("<color=grey>" + "SaferPasteDispenser.MultipleFeeds.description".Translate(TraitDefOf.Cannibal.degreeDatas[0].label, "Mood".Translate()) + "</color>");
			Text.Font = GameFont.Small;

			listing.End();
		}
	}
}
