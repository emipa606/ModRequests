using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SaferPasteDispenser
{
	internal class ThreadSafeFoodUtility
	{
		public struct ThoughtFromIngesting
		{
			public ThoughtDef thought;

			public Precept fromPrecept;
		}

		internal static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, ref List<ThoughtFromIngesting> ingestThoughts, out bool ateFungus, out bool ateNonFungusRawPlant)
		{
			ateFungus = false;
			ateNonFungusRawPlant = false;
			if (ingredient.ingestible == null)
			{
				return;
			}
			MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(ingredient);
			List<ThoughtDef> extraIngestThoughtsFromTraits = new List<ThoughtDef>();
			ingester.story?.traits?.GetExtraThoughtsFromIngestion(extraIngestThoughtsFromTraits, ingredient, meatSourceCategory, direct: false);
			for (int i = 0; i < extraIngestThoughtsFromTraits.Count; i++)
			{
				TryAddIngestThought(ingester, extraIngestThoughtsFromTraits[i], null, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			if (ingredient.ingestible.specialThoughtAsIngredient != null)
			{
				TryAddIngestThought(ingester, ingredient.ingestible.specialThoughtAsIngredient, null, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			if (meatSourceCategory == MeatSourceCategory.Humanlike)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeat, ingester, ref ingestThoughts, ingredient, meatSourceCategory);
				AddThoughtsFromIdeo(HistoryEventDefOf.AteHumanMeatAsIngredient, ingester, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			else if (FoodUtility.IsVeneratedAnimalMeatOrCorpse(ingredient, ingester))
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteVeneratedAnimalMeat, ingester, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			if (meatSourceCategory == MeatSourceCategory.Insect)
			{
				AddThoughtsFromIdeo(HistoryEventDefOf.AteInsectMeatAsIngredient, ingester, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			if (ModsConfig.IdeologyActive && ingredient.thingCategories != null && ingredient.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
			{
				if (ingredient.IsFungus)
				{
					AddThoughtsFromIdeo(HistoryEventDefOf.AteFungusAsIngredient, ingester, ref ingestThoughts, ingredient, meatSourceCategory);
					ateFungus = true;
				}
				else
				{
					ateNonFungusRawPlant = true;
				}
			}
		}

		private static void TryAddIngestThought(Pawn ingester, ThoughtDef def, Precept fromPrecept, ref List<ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
		{
			ThoughtFromIngesting thoughtFromIngesting = default(ThoughtFromIngesting);
			thoughtFromIngesting.thought = def;
			thoughtFromIngesting.fromPrecept = fromPrecept;
			ThoughtFromIngesting item = thoughtFromIngesting;
			if (ingester.story != null && ingester.story.traits != null)
			{
				if (!ingester.story.traits.IsThoughtFromIngestionDisallowed(def, foodDef, meatSourceCategory))
				{
					ingestThoughts.Add(item);
				}
			}
			else
			{
				ingestThoughts.Add(item);
			}
		}

		private static void AddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ref List<ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
		{
			if (ingester.Ideo == null)
			{
				return;
			}
			List<Precept> preceptsListForReading = ingester.Ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				List<PreceptComp> comps = preceptsListForReading[i].def.comps;
				for (int j = 0; j < comps.Count; j++)
				{
					PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought;
					if ((preceptComp_SelfTookMemoryThought = comps[j] as PreceptComp_SelfTookMemoryThought) != null && preceptComp_SelfTookMemoryThought.eventDef == eventDef)
					{
						TryAddIngestThought(ingester, preceptComp_SelfTookMemoryThought.thought, preceptsListForReading[i], ref ingestThoughts, foodDef, meatSourceCategory);
					}
				}
			}
		}
	}
}
