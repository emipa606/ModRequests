using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using VarietyMatters;
using Verse;

namespace VarietyMattersMoreCompat
{
    [HotSwappable]
    public static class HarmonyPatches
    {
        [HotSwappable]
        [HarmonyPatch(typeof(Pawn_VarietyTracker), nameof(Pawn_VarietyTracker.TrackVarieties))]
        public static class Pawn_VarietyTracker_TrackVarieties
        {
            private static Type stackByCondimentType = AccessTools.TypeByName("VanillaCookingExpanded.CompStackByCondiments");

            [UsedImplicitly]
            private static void Postfix(ref Pawn_VarietyTracker pawnRecord, Pawn ingester, Thing foodSource)
            {
                if (ModSettings_VarietyMatters.ignoreIngredients)
                    return;

                var dessertQuality = DefLists.desserts.TryGetValue(foodSource.def, -1);
                var preferability = foodSource.def.ingestible.preferability;

                if (preferability <= FoodPreferability.RawBad && dessertQuality < 0)
                    return;

                var ingredients = foodSource.TryGetComp<CompIngredients>();
                if (ingredients == null)
                    return;

                var settings = VarietyMattersMoreCompatMod.Settings;
                var condiments = !settings.betterGourmet || stackByCondimentType == null ? null : (foodSource as ThingWithComps)?.AllComps.FirstOrDefault(x => stackByCondimentType.IsInstanceOfType(x));
                var baseExpectation = !settings.fixDesserts ? -1 : (int)VarietyExpectation.GetBaseVarietyExpectation(ingester);
                var isArchotech = settings.betterArchotech && DefLists.archotechs.Contains(foodSource.def);

                if (!isArchotech && dessertQuality <= 0 && condiments == null)
                    return;

                if (ingredients.ingredients.Count == 0 && !ModSettings_VarietyMatters.maxVariety)
                {
                    if (isArchotech)
                    {
                        pawnRecord.recentlyConsumed.Add($"Mystery archotech{Rand.Range(0, baseExpectation / 2)}");
                        pawnRecord.lastVariety.Add("archotech ingredient");
                    }

                    if (condiments != null)
                    {
                        pawnRecord.recentlyConsumed.Add($"Mystery gourmet{Rand.Range(0, baseExpectation / 2)}");
                        pawnRecord.lastVariety.Add("gourmet ingredient");
                    }

                    switch (dessertQuality)
                    {
                        case <= 0:
                            return;
                        case >= 2:
                            pawnRecord.recentlyConsumed.Add($"Mystery lavish{Rand.Range(0, baseExpectation / 2)}");
                            pawnRecord.lastVariety.Add("lavish ingredient");
                            break;
                    }

                    pawnRecord.recentlyConsumed.Add($"Mystery meat{Rand.Range(0, baseExpectation / 2)}");
                    pawnRecord.lastVariety.Add("fine ingredient");

                    return;
                }

                foreach (var ingredientDef in ingredients.ingredients)
                {
                    if (!ingredientDef.IsNutritionGivingIngestible)
                        continue;

                    var label = ingredientDef.label;

                    void RemoveFood(ref Pawn_VarietyTracker record)
                    {
                        if (record.lastVariety.Remove($"lavishly prepared {label}"))
                            record.recentlyConsumed.Remove($"Lavish {label}");
                        else if (record.lastVariety.Remove(label))
                            record.recentlyConsumed.Remove(label);
                    }

                    // Upgrade archotech meals
                    if (isArchotech)
                    {
                        var archotechLabel = $"Archotech{label}";
                        if (!pawnRecord.recentlyConsumed.Contains(archotechLabel))
                        {
                            RemoveFood(ref pawnRecord);
                            pawnRecord.recentlyConsumed.Add(archotechLabel);
                            pawnRecord.lastVariety.Add($"archotech quality {label}");

                            continue;
                        }
                    }

                    // Upgrade gourmet meals
                    if (condiments != null || isArchotech)
                    {
                        var gourmetLabel = $"Gourmet{label}";
                        if (!pawnRecord.recentlyConsumed.Contains(gourmetLabel))
                        {
                            RemoveFood(ref pawnRecord);
                            pawnRecord.recentlyConsumed.Add(gourmetLabel);
                            pawnRecord.lastVariety.Add($"gourmet prepared {label}");

                            continue;
                        }
                    }

                    // Upgrade lavish desserts
                    if (dessertQuality >= 3)
                    {
                        var lavishLabel = $"Lavish{label}";
                        if (!pawnRecord.recentlyConsumed.Contains(lavishLabel))
                        {
                            RemoveFood(ref pawnRecord);
                            pawnRecord.recentlyConsumed.Add(lavishLabel);
                            pawnRecord.lastVariety.Add($"lavishly prepared {label}");

                            continue;
                        }
                    }

                    if (!pawnRecord.recentlyConsumed.Contains(label))
                    {
                        pawnRecord.recentlyConsumed.Add(label);
                        pawnRecord.lastVariety.Add(label);
                    }
                }
            }
        }
    }
}
