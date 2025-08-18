using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SaferPasteDispenser
{
	static class ExtensionMethod
	{
		public static Thing TryDispenseSaferFood(this Building_NutrientPasteDispenser dispenser, Pawn actor)
		{
			if (!dispenser.CanDispenseNow)
			{
				return null;
			}

			// NPDTiers patch
			if (dispenser.IsNPDTiers())
			{
				return dispenser.TryDispenseFood();
			}

			var target = actor.jobs.curDriver.job.targetB;
			var eater = target.Pawn is null ? actor : target.Pawn;

			float nutritionCost = dispenser.def.building.nutritionCostPerDispense - 0.0001f;
			float nutritionCostLeft = nutritionCost;
			List<ThingDef> ingredients = [];

			// Allow multiple ingredients
			if (Settings.numberOfIngredients != Settings.NumberOfIngredients.Single)
			{
				var feedGroups = from feed in dispenser.FindFeedsAmongAllHoppers()
								 let mood = SaferPasteUtility.MoodEffectFromIngredient(eater, feed)
								 where mood >= 0f
								 orderby -mood, feed.TryGetComp<CompRottable>()?.TicksUntilRotAtTemp(10f) ?? 0, feed.stackCount
								 group (feed, mood) by feed.def.shortHash into groupByFeedDef
								 select groupByFeedDef.First();
				int numberOfIngredients = Mathf.Min((int)Settings.numberOfIngredients, feedGroups.Count());
				foreach (var feedGroup in feedGroups)
				{
					Thing feed = feedGroup.feed;
					float feedNutrition = feed.GetStatValue(StatDefOf.Nutrition);
					int pickedFeedQuantity = Math.Min(feed.stackCount, Mathf.CeilToInt(nutritionCostLeft / feedNutrition));
					if (feed != feedGroups.Last().feed)
					{
						pickedFeedQuantity = Math.Min(pickedFeedQuantity, Mathf.CeilToInt(nutritionCost / feedNutrition / numberOfIngredients));
					}
					nutritionCostLeft -= pickedFeedQuantity * feedNutrition;
					ingredients.Add(feed.def);
					feed.SplitOff(pickedFeedQuantity);
					if (nutritionCostLeft <= 0f)
					{
						break;
					}
				}
			}

			while (nutritionCostLeft > 0f)
			{
				var feeds = dispenser.FindFeedsAmongAllHoppers();
				if (feeds.Count() == 0)
				{
					Log.Error("Did not find enough food in hoppers while trying to dispense.");
					return null;
				}
				Thing bestFeed = eater.PickBestFeed(feeds);
				int pickedFeedQuantity = Math.Min(bestFeed.stackCount, Mathf.CeilToInt(nutritionCostLeft / bestFeed.GetStatValue(StatDefOf.Nutrition)));
				nutritionCostLeft -= pickedFeedQuantity * bestFeed.GetStatValue(StatDefOf.Nutrition);
				ingredients.Add(bestFeed.def);
				bestFeed.SplitOff(pickedFeedQuantity);
			}
			dispenser.def.building.soundDispense.PlayOneShot(new TargetInfo(dispenser.Position, dispenser.Map));
			Thing paste = ThingMaker.MakeThing(ThingDefOf.MealNutrientPaste);
			CompIngredients compIngredients = paste.TryGetComp<CompIngredients>();
			for (int i = 0; i < ingredients.Count; i++)
			{
				compIngredients.RegisterIngredient(ingredients[i]);
			}
			return paste;

			/*
			IEnumerable<Thing> FindFeedsAmongAllHoppers()
			{
				var feeds = new List<Thing>();
				foreach (var AdjCells in dispenser.AdjCellsCardinalInBounds)
				{
					bool isHopper = false;
					Thing feed = null;
					foreach (var adjThing in AdjCells.GetThingList(dispenser.Map))
					{
						if (adjThing.IsHopper())
						{
							isHopper = true;
						}
						else if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(adjThing.def))
						{
							feed = adjThing;
						}
					}
					if (isHopper && feed != null)
					{
						feeds.Add(feed);
					}
				}
				return feeds;
			}
			*/
		}

		internal static IEnumerable<Thing> FindFeedsAmongAllHoppers(this Building_NutrientPasteDispenser dispenser)
		{
			List<Thing> feeds = [];
			bool isHopper = false;
			foreach (IntVec3 AdjCells in dispenser.AdjCellsCardinalInBounds)
			{
				foreach (Thing adjThing in AdjCells.GetThingList(dispenser.Map))
				{
					if (adjThing.IsHopper() || adjThing.IsNPDTiers())
					{
						isHopper = true;
					}
					else if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(adjThing.def))
					{
						feeds.Add(adjThing);
					}
				}
			}
			return isHopper ? feeds : null;
		}

		public static Thing PickBestFeed(this Pawn eater, IEnumerable<Thing> feeds)
		{
			Thing bestFeed = feeds.First();
			var bestValue = float.MinValue;
			var ticksUntilRot = int.MaxValue;
			foreach (var feed in feeds)
			{
				var value = FoodUtility.FoodOptimality(eater, feed, feed.def, 0);
				if (bestValue < value)
				{
					bestFeed = feed;
					bestValue = value;
				}
				else if (bestValue == value)
				{
					if (feed.TryGetComp<CompRottable>() is CompRottable rottable)
					{
						var ticksUntilRotTemp = rottable.TicksUntilRotAtTemp(10f);
						if (ticksUntilRot / 500 > ticksUntilRotTemp / 500)
						{
							bestFeed = feed;
							bestValue = value;
							ticksUntilRot = ticksUntilRotTemp;
						}
						else if (ticksUntilRot / 500 == ticksUntilRotTemp / 500)
						{
							if (bestFeed.stackCount > feed.stackCount)
							{
								bestFeed = feed;
								bestValue = value;
							}
						}
					}
				}
			}
			return bestFeed;
		}
	}

	static class ExtensionMethodNPDTiers
	{
		public static bool IsNPDTiers(this Building_NutrientPasteDispenser dispenser)
		{
			return SaferPasteUtility.IsNPDTiers() && SaferPasteUtility.IsNPDTiers(dispenser);
		}

		private static Type cacheNPDHopper_Storage = null;
		private static bool hasChachedNPDHopper_Storage = false;

		internal static bool IsNPDTiers(this Thing hopper)
		{
			if (!SaferPasteUtility.IsNPDTiers())
			{
				return false;
			}
			if (!hasChachedNPDHopper_Storage)
			{
				cacheNPDHopper_Storage = AccessTools.TypeByName("NutrientPasteTiers.NPDHopper_Storage");
				hasChachedNPDHopper_Storage = true;
			}
			return hopper.GetType() == cacheNPDHopper_Storage;
		}
	}

	static class ExtensionMethodReplimat
	{
		private static Type cacheReplimat_Building = null;
		private static bool hasChachedReplimat_Building = false;

		internal static bool IsReplimat(this Thing terminal)
		{
			if (!SaferPasteUtility.IsReplimat())
			{
				return false;
			}
			if (!hasChachedReplimat_Building)
			{
				cacheReplimat_Building = AccessTools.TypeByName("Replimat.Building_ReplimatTerminal");
				hasChachedReplimat_Building = true;
			}
			return terminal.GetType() == cacheReplimat_Building;
		}
	}

	static class ExtensionMethodVNPE
	{
		private static Type cacheVNPE_Building = null;
		private static bool hasChachedVNPE_Building = false;

		internal static bool IsVNPE(this Thing terminal)
		{
			if (!SaferPasteUtility.IsVNPE())
			{
				return false;
			}
			if (!hasChachedVNPE_Building)
			{
				cacheVNPE_Building = AccessTools.TypeByName("VNPE.Building_NutrientPasteTap");
				hasChachedVNPE_Building = true;
			}
			return terminal.GetType() == cacheVNPE_Building;
		}
	}

	static class ExtensionMethodIncompatible
	{
		internal static bool IsIncompatibleMod(this Thing terminal)
		{
			return terminal.IsReplimat() || terminal.IsVNPE();
		}
	}
}
