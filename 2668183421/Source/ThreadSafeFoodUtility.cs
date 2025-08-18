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
		internal static void AddIngestThoughtsFromIngredient(ThingDef ingredient, Pawn ingester, ref List<SaferPasteUtility.ThoughtFromIngesting> ingestThoughts, out bool ateFungus, out bool ateNonFungusRawPlant)
		{
			ateFungus = false;
			ateNonFungusRawPlant = false;
			if (ingredient.ingestible is null)
			{
				return;
			}
			MeatSourceCategory meatSourceCategory = FoodUtility.GetMeatSourceCategory(ingredient);
			List<ThoughtDef> extraIngestThoughtsFromTraits = [];
			ingester.story?.traits?.GetExtraThoughtsFromIngestion(extraIngestThoughtsFromTraits, ingredient, meatSourceCategory, direct: false);
			for (int i = 0; i < extraIngestThoughtsFromTraits.Count; i++)
			{
				TryAddIngestThought(ingester, extraIngestThoughtsFromTraits[i], null, ref ingestThoughts, ingredient, meatSourceCategory);
			}
			if (ingredient.ingestible.specialThoughtAsIngredient is not null)
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
			if (ModsConfig.IdeologyActive && ingredient.thingCategories is not null && ingredient.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw))
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

		internal static void TryAddIngestThought(Pawn ingester, ThoughtDef def, Precept fromPrecept, ref List<SaferPasteUtility.ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
		{
			if (ThoughtUtility.NullifyingGene(def, ingester) is not null || ThoughtUtility.NullifyingHediff(def, ingester) is not null || ThoughtUtility.NullifyingPrecept(def, ingester) is not null || ThoughtUtility.NullifyingTrait(def, ingester) is not null || (fromPrecept is not null && !fromPrecept.def.enabledForNPCFactions && !ingester.CountsAsNonNPCForPrecepts()))
			{
				return;
			}
			SaferPasteUtility.ThoughtFromIngesting thoughtFromIngesting = default;
			thoughtFromIngesting.thought = def;
			thoughtFromIngesting.fromPrecept = fromPrecept;
			SaferPasteUtility.ThoughtFromIngesting item = thoughtFromIngesting;
			if (ingester.story is not null && ingester.story.traits is not null)
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

		internal static void AddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ref List<SaferPasteUtility.ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
		{
			if (ingester.Ideo is null)
			{
				return;
			}
			List<Precept> preceptsListForReading = ingester.Ideo.PreceptsListForReading;
			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
				List<PreceptComp> comps = preceptsListForReading[i].def.comps;
				for (int j = 0; j < comps.Count; j++)
				{
					if (comps[j] is PreceptComp_SelfTookMemoryThought preceptComp_SelfTookMemoryThought && preceptComp_SelfTookMemoryThought.eventDef == eventDef)
					{
						TryAddIngestThought(ingester, preceptComp_SelfTookMemoryThought.thought, preceptsListForReading[i], ref ingestThoughts, foodDef, meatSourceCategory);
					}
				}
			}
		}
	}
}
