using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GenRecipeMakeRecipeProducts_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, Thing dominantIngredient)
        {
            if (!worker.IsColonyMech)
            {
                RandomProductExtension rpEx = recipeDef.GetModExtension<RandomProductExtension>();
                float pawnsAvgSkillLevel = worker.skills.AverageOfRelevantSkillsFor(worker.CurJob.workGiverDef.workType);
                float skillsFactor = 0.20f;

                List<Thing> modifiedProducts = new();

                foreach (Thing product in __result)
                {
                    if (rpEx != null && rpEx.randomProducts != null && rpEx.randomProductChance.HasValue && rpEx.randomProductChance.Value > 0f 
                        && Rand.Chance(rpEx.randomProductChance.Value))
                    {
                        float totalWeight = 0f;
                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            totalWeight += productData.randomProductWeight;
                        }

                        float randomValue = Rand.Range(0f, totalWeight);
                        float accumulatedWeight = 0f;

                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            accumulatedWeight += productData.randomProductWeight;

                            if (randomValue <= accumulatedWeight)
                            {
                                ThingDef selectedDef = DefDatabase<ThingDef>.GetNamed(productData.randomProduct.defName, errorOnFail: false);
                                if (selectedDef != null)
                                {
                                    if (recipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        for (int i = 0; i < rpEx.randomProducts.Count; i++)
                                        {
                                            if (product.def == rpEx.randomProducts[i].randomProduct)
                                            {
                                                product.def = rpEx.randomProducts[i].randomProduct;

                                                SimpleCurve bonusSpawnCurve = new()
                                                {
                                                    { 0, 0 },
                                                    { 5, 0 },
                                                    { 8, Rand.RangeInclusive(0, 1) },
                                                    { 14, Rand.RangeInclusive(0, 2) },
                                                    { 18, Rand.RangeInclusive(0, 3) },
                                                    { 20, Rand.RangeInclusive(0, 4) }
                                                };

                                                int bonusSpawnCount = (int)bonusSpawnCurve.Evaluate(pawnsAvgSkillLevel);
                                                int finalSpawnCount = product.stackCount + bonusSpawnCount;
                                                product.stackCount = finalSpawnCount;

                                                if (bonusSpawnCount > 0 && RandomChanceSettings.AllowMessages)
                                                {
                                                    Messages.Message("RC_BonusMealProduct".Translate(worker.Named("PAWN"), 
                                                        product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        product.def = selectedDef;
                                        FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                                        int skillBasedSpawnCount = Mathf.RoundToInt(pawnsAvgSkillLevel * skillsFactor);

                                        FloatRange newMinSpawnCountRange;
                                        FloatRange newMaxSpawnCountRange;

                                        newMinSpawnCountRange = new FloatRange(initialSpawnCountRange.min, initialSpawnCountRange.min * skillBasedSpawnCount);
                                        newMaxSpawnCountRange = new FloatRange(initialSpawnCountRange.max, initialSpawnCountRange.max * skillBasedSpawnCount);

                                        int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, (int)newMinSpawnCountRange.max);
                                        int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, (int)newMaxSpawnCountRange.max);

                                        product.stackCount = Rand.RangeInclusive(newMinSpawnCount, newMaxSpawnCount);

                                        if (RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_RandomStoneCuttingProduct".Translate(worker.Named("PAWN"),
                                            dominantIngredient.Label, product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }

                    if (recipeDef == RandomChance_DefOf.ButcherCorpseFlesh && Rand.Chance(RandomChanceSettings.BonusButcherProductChance))
                    {
                        Thing butcheredCorpse = worker.CurJob.GetTarget(TargetIndex.B).Thing;

                        SimpleCurve chanceCurve = new()
                        {
                            { 0, 0.025f },
                            { 3, 0.05f },
                            { 6, 0.09f },
                            { 8, 0.2f },
                            { 14, 0.3f },
                            { 18, 0.4f },
                            { 20, 0.5f }
                        };

                        if (butcheredCorpse is Corpse corpse)
                        {
                            if (corpse.InnerPawn.RaceProps.predator)
                            {
                                if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                                {
                                    int additionalMeatStackCount = Rand.RangeInclusive(1, 15);
                                    float butcheredCorpseBodySizeFactor = (corpse.InnerPawn.RaceProps.baseBodySize / 1.15f);
                                    butcheredCorpseBodySizeFactor = Mathf.Max(butcheredCorpseBodySizeFactor, 1f);

                                    ThingDef meatDef = GetRandomMeatFromOtherPawn();
                                    Thing additionalMeat = ThingMaker.MakeThing(meatDef);
                                    additionalMeat.stackCount = additionalMeatStackCount * Mathf.RoundToInt(butcheredCorpseBodySizeFactor);

                                    if (RandomChanceSettings.AllowMessages)
                                    {
                                        Messages.Message("RC_PredatorButcheryBonusProduct".Translate(worker.Named("PAWN"),
                                        additionalMeat.Label, dominantIngredient.Label), worker, MessageTypeDefOf.PositiveEvent);
                                    }

                                    modifiedProducts.Add(additionalMeat);
                                }
                            }
                        }
                    }
                    modifiedProducts.Add(product);
                }
                __result = modifiedProducts;
            }
        }
        
        private static ThingDef GetRandomMeatFromOtherPawn()
        {
            List<ThingDef> meatDefs = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(def => def.IsWithinCategory(ThingCategoryDefOf.MeatRaw))
                    .ToList();

            if (meatDefs.Count > 0)
                return meatDefs.RandomElement();
            else
                return ThingDefOf.Meat_Human;
        }
    }
}
