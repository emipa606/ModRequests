using System;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using XmlExtensions;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace WealthScalingOrbitalTraders
{
    [HarmonyPatch]
    static class Trader_GenerateThings_Patch
    {
        public static float CalculateAddedItemsDefault(float thisColonyWealth)
        {
            float wealthPerStock = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthPerStock"));
            float wealthMinimum = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMinWealth"));
            float maximumMultiplier = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMaxMultiplier")) / 100f;
            float falloffStartPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartPolynomial")) / 100f;
            float falloffPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffPolynomial")) / 100f;
            float falloffStartTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartTriangular")) / 100f;
            float falloffTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffTriangular")) / 100f;
            float falloffStartLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartLinear")) / 100f;
            float falloffLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffLinear")) / 100f;
            return CalculateAddedItems(thisColonyWealth, wealthPerStock, wealthMinimum, maximumMultiplier, falloffStartPolynomial, falloffPolynomial, falloffStartTriangular, falloffTriangular, falloffStartLinear, falloffLinear);
        }
        public static float CalculateAddedItems(float thisColonyWealth, float wealthPerStock, float maximumMultiplier, float wealthMinimum, float falloffStartPolynomial, float falloffPolynomial, float falloffStartTriangular, float falloffTriangular, float falloffStartLinear, float falloffLinear)
        {


            float addedItemsMultiplier = (thisColonyWealth - wealthMinimum) / wealthPerStock;

            float finalFalloffPolynomial = 1f;
            if (falloffPolynomial > 0 && addedItemsMultiplier > falloffStartPolynomial)
            {
                float n1 = falloffStartPolynomial;
                float n2 = 1;
                float n3 = 1;
                while (addedItemsMultiplier > n1)
                {
                    n1 = n1 + 1f;
                    n3 = n2;
                    n2 = n2 + 1f / (float)Math.Pow(1f + falloffPolynomial, (float)Math.Floor(n2));
                    //linearly interpolate
                    if (addedItemsMultiplier <= n1)
                    {
                        float diff = n2 - n3;
                        finalFalloffPolynomial = (n3 + diff * (addedItemsMultiplier - n1 + 1) + falloffStartPolynomial - 1) / addedItemsMultiplier;
                    }
                }
            }

            float finalFalloffTriangular = 1f;
            if (falloffTriangular > 0 && addedItemsMultiplier > falloffStartTriangular)
            {
                float n1 = falloffStartTriangular;
                float n2 = 1;
                float n3 = 1;
                while (addedItemsMultiplier > n1)
                {
                    n1 = n1 + 1f;
                    n3 = n2;
                    n2 = n2 + 1f / (1f + (float)Math.Floor(n2) * falloffTriangular);
                    //linearly interpolate
                    if (addedItemsMultiplier <= n1)
                    {
                        float diff = n2 - n3;
                        finalFalloffTriangular = (n3 + diff * (addedItemsMultiplier - n1 + 1) + falloffStartTriangular - 1) / addedItemsMultiplier;
                    }
                }
            }

            float finalFalloffLinear = 1f;
            if (falloffLinear > 0 && addedItemsMultiplier > falloffStartLinear)
            {
                finalFalloffLinear = (addedItemsMultiplier - falloffLinear * (Math.Max(falloffStartLinear, addedItemsMultiplier) - falloffStartLinear)) / addedItemsMultiplier;
            }


            addedItemsMultiplier = addedItemsMultiplier * finalFalloffTriangular * finalFalloffLinear * finalFalloffPolynomial;

            if (maximumMultiplier >= 0)
            {
                addedItemsMultiplier = Math.Min(maximumMultiplier, addedItemsMultiplier);
            }
            return addedItemsMultiplier;
        }

        [HarmonyPatch(typeof(ThingSetMaker_TraderStock), nameof(ThingSetMaker_TraderStock.Generate))]
        static void Postfix(ThingSetMakerParams parms, List<Thing> outThings, ThingSetMaker_TraderStock __instance)
        {
            bool scalingActivated = false;
            string applyToThingGeneratorResult = "false";
            SettingsManager.TryGetSetting("scalingorbitaltraders", "applyTo_"+parms.traderDef.defName, out applyToThingGeneratorResult);
            Boolean.TryParse(applyToThingGeneratorResult,out scalingActivated);
            Log.Message("ThingSetMaker_TraderStock: does SWOT apply?: " + scalingActivated.ToString() + " (debug: "+outThings.Count.ToString()+" items in outThings");
            if (scalingActivated == true)
            {
                if (parms.tile != null) //double check
                {
                    float wealthPerStockDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthPerStock"));
                    float wealthMinimumDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMinWealth"));
                    float maximumMultiplierDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMaxMultiplier")) / 100f;
                    float falloffStartPolynomialDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartPolynomial")) / 100f;
                    float falloffPolynomialDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffPolynomial")) / 100f;
                    float falloffStartTriangularDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartTriangular")) / 100f;
                    float falloffTriangularDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffTriangular")) / 100f;
                    float falloffStartLinearDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartLinear")) / 100f;
                    float falloffLinearDefault = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffLinear")) / 100f;
                    TraderKindDef traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement();
                    Faction makingFaction = parms.makingFaction;
                    int forTile = parms.tile.HasValue ? parms.tile.Value : ((Find.AnyPlayerHomeMap != null) ? Find.AnyPlayerHomeMap.Tile : ((Find.CurrentMap == null) ? (-1) : Find.CurrentMap.Tile));
                    //ThingOwner newItems = (Traverse.Create(__instance).Field("things").GetValue<ThingOwner>());
                    List<Thing> newItems = new List<Thing>();
                    for (int i = 0; i < traderKindDef.stockGenerators.Count; i++)
                    {
                        if (traderKindDef.stockGenerators[i].countRange.max > 0)
                        {

                            StockGenerator thisGenerator = traderKindDef.stockGenerators[i];
                            float thisColonyWealth = WealthUtility.PlayerWealth;
                            float alivePawnScaling = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "livingPawnScalingMult")) / 100f;
                            float geneScaling = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "geneScalingMult")) / 100f;
                            float trainerScaling = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "trainerScalingMult")) / 100f;


                            float wealthPerStock = wealthPerStockDefault;
                            float wealthMinimum = wealthMinimumDefault;
                            float maximumMultiplier = maximumMultiplierDefault;
                            float falloffStartPolynomial = falloffStartPolynomialDefault;
                            float falloffPolynomial = falloffPolynomialDefault;
                            float falloffStartTriangular = falloffStartTriangularDefault;
                            float falloffTriangular = falloffTriangularDefault;
                            float falloffStartLinear = falloffStartLinearDefault;
                            float falloffLinear = falloffLinearDefault;

                            if (bool.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "overrideReq_" + traderKindDef.defName)))
                            {
                                wealthPerStock = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthPerStock_" + traderKindDef.defName));
                                wealthMinimum = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMinWealth_" + traderKindDef.defName));
                                maximumMultiplier = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "scalingMaxMultiplier_" + traderKindDef.defName));
                            }
                            if (bool.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "override_" + traderKindDef.defName)))
                            {
                                falloffStartPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartPolynomial_" + traderKindDef.defName)) / 100f;
                                falloffPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffPolynomial_" + traderKindDef.defName)) / 100f;
                                falloffStartTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartTriangular_" + traderKindDef.defName)) / 100f;
                                falloffTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffTriangular_" + traderKindDef.defName)) / 100f;
                                falloffStartLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffStartLinear_" + traderKindDef.defName)) / 100f;
                                falloffLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "wealthFalloffLinear_" + traderKindDef.defName)) / 100f;
                            }
                            string EntryString = thisGenerator.GetType().ToString() + "_";

                            //dark wizardry ahead
                            if (thisGenerator.GetType().GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance) != null)
                            {
                                EntryString = EntryString + thisGenerator.GetType().GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thisGenerator);
                            }
                            else if (thisGenerator.GetType().GetField("categoryDef", BindingFlags.NonPublic | BindingFlags.Instance) != null)
                            {
                                EntryString = EntryString + thisGenerator.GetType().GetField("categoryDef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thisGenerator);
                            }
                            else if (thisGenerator.GetType().GetField("tradeTag", BindingFlags.NonPublic | BindingFlags.Instance) != null)
                            {
                                EntryString = EntryString + thisGenerator.GetType().GetField("tradeTag", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thisGenerator);
                            }
                            EntryString = EntryString.Replace("RimWorld.", "");
                            EntryString = EntryString + "_Entry";
                            bool hasSetting = false;
                            string settingApplied = "false";
                            SettingsManager.TryGetSetting("scalingorbitaltraders", EntryString, out settingApplied);
                            Boolean.TryParse(settingApplied, out hasSetting);
                            Log.Message(EntryString);

                            if (hasSetting)
                            {
                                Log.Message("Has setting: " + EntryString);
                                bool oReq = false;
                                string oReqStr = "false";
                                SettingsManager.TryGetSetting("scalingorbitaltraders", "itemOverrideReq_" + EntryString, out oReqStr);
                                Boolean.TryParse(oReqStr, out oReq);
                                if (oReq)
                                {
                                    wealthPerStock = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthPerStock_" + EntryString));
                                    wealthMinimum = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemScalingMinWealth_" + EntryString));
                                    maximumMultiplier = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemScalingMaxMultiplier_" + EntryString));
                                }
                                bool iOvr = false;
                                string iOvrStr = "false";
                                SettingsManager.TryGetSetting("scalingorbitaltraders", "itemOverride_" + EntryString, out iOvrStr);
                                Boolean.TryParse(oReqStr, out iOvr);
                                if (iOvr)
                                {
                                    falloffStartPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffStartPolynomial_" + EntryString)) / 100f;
                                    falloffPolynomial = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffPolynomial_" + EntryString)) / 100f;
                                    falloffStartTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffStartTriangular_" + EntryString)) / 100f;
                                    falloffTriangular = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffTriangular_" + EntryString)) / 100f;
                                    falloffStartLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffStartLinear_" + EntryString)) / 100f;
                                    falloffLinear = float.Parse(SettingsManager.GetSetting("scalingorbitaltraders", "itemWealthFalloffLinear_" + EntryString)) / 100f;
                                }
                            }
                            float addedItemsMultiplier = CalculateAddedItems(thisColonyWealth, wealthPerStock, wealthMinimum, maximumMultiplier, falloffStartPolynomial, falloffPolynomial, falloffStartTriangular, falloffTriangular, falloffStartLinear, falloffLinear);
                            if (addedItemsMultiplier > 0.01f)
                            {
                                for (float j = 0; j < addedItemsMultiplier; j = j + 1f)
                                {
                                    IEnumerable<Thing> GeneratedThings = thisGenerator.GenerateThings(forTile, parms.makingFaction);
                                    long itemCount = 0;
                                    foreach (Thing item in GeneratedThings)
                                    {
                                        itemCount = itemCount + 1;
                                    }
                                    float itemCutoffProportional = addedItemsMultiplier - j;
                                    long itemCutoff = 0;
                                    if (j + 1f > addedItemsMultiplier)
                                    {
                                        itemCutoff = (long)Math.Round(itemCount * itemCutoffProportional);
                                    }
                                    FloatRange randomnessCutoff = new FloatRange(0f, 1f);
                                    long itemCutoffPawns = (long)Math.Floor((float)itemCount - ((float)itemCount - (float)itemCutoff) * alivePawnScaling + randomnessCutoff.RandomInRange);
                                    long itemCutoffGenes = (long)Math.Floor((float)itemCount - ((float)itemCount - (float)itemCutoff) * geneScaling + randomnessCutoff.RandomInRange);
                                    long itemCutoffTrainers = (long)Math.Floor((float)itemCount - ((float)itemCount - (float)itemCutoff) * trainerScaling + randomnessCutoff.RandomInRange);
                                    foreach (Thing item in GeneratedThings)
                                    {
                                        if (!item.def.tradeability.TraderCanSell())
                                        {
                                            Log.Warning("(Orbital Trader Wealth Scaling) " + traderKindDef + " generated carrying " + item + " which can't be sold by traders. Ignoring...");
                                        }
                                        else
                                        {
                                            bool isGene = false;
                                            if (item.def.thingClass.IsAssignableFrom(typeof(GeneSetHolderBase)))
                                            {
                                                isGene = true;
                                            }
                                            bool isTrainer = (item.HasThingCategory(ThingCategoryDefOf.Neurotrainers) || item.HasThingCategory(ThingCategoryDefOf.NeurotrainersSkill) || item.HasThingCategory(ThingCategoryDefOf.NeurotrainersPsycast));
                                            if (itemCount > itemCutoff)
                                            {
                                                if ((item as Pawn) == null || itemCount > itemCutoffPawns)
                                                {
                                                    if (!isGene || itemCount > itemCutoffGenes)
                                                    {
                                                        if (!isTrainer || itemCount > itemCutoffTrainers)
                                                        {
                                                            item.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
                                                            newItems.Add(item);
                                                            //newItems.TryAddOrTransfer(item);
                                                            //Traverse.Create().Field().SetValue() used to be here
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Harmony is really cool
                    //Traverse.Create(__instance).Field("things").SetValue(newItems);
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        bool found = false;
                        for (int j = 0; j < outThings.Count && found == false; j++)
                        {
                            if (newItems[i].CanStackWith(outThings[j]) && outThings[j].CanStackWith(newItems[i]))
                            {
                                outThings[j].TryAbsorbStack(newItems[i], false);
                                found = true;
                            }
                        }
                        if (found == false)
                        {
                            outThings.Add(newItems[i]);
                        }
                    }
                }
                else
                {
                    Log.Warning("Wealth Scaling Orbital Traders: Tile was null.");
                }
            }
        }
    }
}
