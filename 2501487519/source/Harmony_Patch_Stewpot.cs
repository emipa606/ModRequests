using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AOBAStew
{
   
    //Setting the Harmony instance

    class Harmony_Patch_StewPot
    {
        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("CanDispenseNow", MethodType.Getter)]
        static class Patch_Building_NutrientPasteDispenser_CanDispenseNow_Getter
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref bool __result)
            {
                //Log.Error("Patch_Building_NutrientPasteDispenser_CanDispenseNow_Getter");

                if (__instance.def.defName == "FT_StewPot")
                {
                    if (!__result)
                    {
                        if (__instance.GetComp<CompRefuelable>().HasFuel)
                            __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("AdjacentReachableHopper")]
        static class Patch_Building_NutrientPasteDispenser_AdjacentReachableHopper
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Building __result, Pawn reacher)
            {
                //Log.Error("Patch_Building_NutrientPasteDispenser_AdjacentReachableHopper");

                if (__instance.def.defName == "FT_StewPot" && __result == null)
                {
                    for (int index = 0; index < __instance.AdjCellsCardinalInBounds.Count; ++index)
                    {
                        Building edifice = __instance.AdjCellsCardinalInBounds[index].GetEdifice(__instance.Map);
                        if (edifice != null && edifice.def == AOBADefOf.FT_FoodTable && reacher.CanReach((LocalTargetInfo)((Thing)edifice), PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                        { 
                            __result = edifice;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Alert_PasteDispenserNeedsHopper))]
        [HarmonyPatch("BadDispensers", MethodType.Getter)]
        static class Patch_Alert_PasteDispenserNeedsHopper_BadDispensers_Get
        {
            static void Postfix(Alert_PasteDispenserNeedsHopper __instance, ref List<Thing> __result)
            {
                if (__result.Count > 0)
                {
                    List<Thing> StewPotsToRemoveFromList = new List<Thing>();

                    foreach (Thing foodBuilding in __result)
                    {
                        if (foodBuilding.def != null && foodBuilding.def.Equals(AOBADefOf.FT_StewPot))
                        {
                            bool flag = false;
                            ThingDef FoodTable = AOBADefOf.FT_FoodTable;
                            foreach (IntVec3 cellsCardinalInBound in ((Building_StewPot)foodBuilding).AdjCellsCardinalInBounds)
                            {
                                Thing edifice = (Thing)cellsCardinalInBound.GetEdifice(foodBuilding.Map);
                                if (edifice != null && edifice.def == FoodTable)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                                StewPotsToRemoveFromList.Add(foodBuilding);
                        }
                    }

                    foreach(Thing thing in StewPotsToRemoveFromList)
                    {
                        if (__result.Contains(thing))
                            __result.Remove(thing);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("TryDispenseFood")]
        static class Patch_Building_NutrientPasteDispenser_TryDispenseFood
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Thing __result)
            {
               //Log.Error("Patch_Building_NutrientPasteDispenser_TryDispenseFood");

                if (__instance.def.defName == "FT_StewPot" && __result == null)
                {
                    Building_StewPot StewPot = __instance as Building_StewPot;

                    //Log.Error("TryDispenseFood HARMONY");
                    if (!StewPot.CanDispenseNow)
                        return;
                    float num = __instance.def.building.nutritionCostPerDispense - 0.0001f;
                    List<ThingDef> thingDefList = new List<ThingDef>();
                    do
                    {
                        Thing feedInAnyHopper = StewPot.FindFeedInAnyHopper();
                        if (feedInAnyHopper == null)
                        {
                           //Log.Error("Did not find enough food in slop pails for a StewPot meal.", false);
                            return;
                        }
                        int count = Mathf.Min(feedInAnyHopper.stackCount, Mathf.CeilToInt(num / feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true)));
                        num -= (float)count * feedInAnyHopper.GetStatValue(StatDefOf.Nutrition, true);
                        thingDefList.Add(feedInAnyHopper.def);
                        feedInAnyHopper.SplitOff(count);
                    }
                    while ((double)num > 0.0);
                    __instance.def.building.soundDispense.PlayOneShot((SoundInfo)new TargetInfo(__instance.Position, __instance.Map, false));
                    Thing thing = ThingMaker.MakeThing(StewPot.DispensableDef, (ThingDef)null);
                    CompIngredients comp = thing.TryGetComp<CompIngredients>();
                    for (int index = 0; index < thingDefList.Count; ++index)
                        comp.RegisterIngredient(thingDefList[index]);
                    __result = thing;
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("FindFeedInAnyHopper")]
        static class Patch_Building_NutrientPasteDispenser_FindFeedInAnyHopper
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref Thing __result)
            {
                //Log.Error("Patch_Building_NutrientPasteDispenser_FindFeedInAnyHopper");

                if (__instance.def.defName == "FT_StewPot" && __result == null)
                {
                    //Log.Error("FindFeedInAnyHopper HARMONY");
                    for (int index1 = 0; index1 < __instance.AdjCellsCardinalInBounds.Count; ++index1)
                    {
                        Thing thing1 = (Thing)null;
                        Thing thing2 = (Thing)null;
                        List<Thing> thingList = __instance.AdjCellsCardinalInBounds[index1].GetThingList(__instance.Map);
                        for (int index2 = 0; index2 < thingList.Count; ++index2)
                        {
                            Thing thing3 = thingList[index2];
                            if (Building_StewPot.IsAcceptableFeedstock(thing3.def))
                                thing1 = thing3;
                            if (thing3.def == AOBADefOf.FT_FoodTable)
                                thing2 = thing3;
                        }
                        if (thing1 != null && thing2 != null)
                        {
                            __result = thing1;
                            return;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.Building_NutrientPasteDispenser))]
        [HarmonyPatch("HasEnoughFeedstockInHoppers")]
        static class Patch_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers
        {
            static void Postfix(Building_NutrientPasteDispenser __instance, ref bool __result)
            {
                //Log.Error("Patch_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers");

                if (__instance.def.defName == "FT_StewPot" && __result == false)
                {
                    //Log.Error("HasEnoughFeedstockInHoppers Harmony");
                    float num = 0.0f;
                    Building_StewPot StewPot = __instance as Building_StewPot;
                    for (int index1 = 0; index1 < __instance.AdjCellsCardinalInBounds.Count; ++index1)
                    {
                        IntVec3 cellsCardinalInBound = __instance.AdjCellsCardinalInBounds[index1];
                        Thing thing1 = (Thing)null;
                        Thing thing2 = (Thing)null;
                        Map map = __instance.Map;
                        List<Thing> thingList = cellsCardinalInBound.GetThingList(map);
                        for (int index2 = 0; index2 < thingList.Count; ++index2)
                        {
                            Thing thing3 = thingList[index2];
                            if (Building_StewPot.IsAcceptableFeedstock(thing3.def))
                                thing1 = thing3;
                            if (thing3.def == AOBADefOf.FT_FoodTable)
                                thing2 = thing3;
                        }
                        if (thing1 != null && thing2 != null)
                            num += (float)thing1.stackCount * thing1.GetStatValue(StatDefOf.Nutrition, true);
                        if ((double)num >= (double)StewPot.def.building.nutritionCostPerDispense)
                        {
                            __result = true;
                            return;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("BestFoodSourceOnMap")]
        static class Patch_FoodUtility_BestFoodSourceOnMap
        {
            static void Postfix(ref Thing __result,
                  Pawn getter,
                  Pawn eater,
                  bool desperate,
                  ref ThingDef foodDef,
                  FoodPreferability maxPref = FoodPreferability.MealLavish,
                  bool allowPlant = true,
                  bool allowDrug = true,
                  bool allowCorpse = true,
                  bool allowDispenserFull = true,
                  bool allowDispenserEmpty = true,
                  bool allowForbidden = false,
                  bool allowSociallyImproper = false,
                  bool allowHarvest = false,
                  bool forceScanWholeMap = false,
                  bool ignoreReservations = false,
                  FoodPreferability minPrefOverride = FoodPreferability.Undefined)
            {
                //Log.Error("Patch_FoodUtility_BestFoodSourceOnMap:" + (__result == null ? "NULL" : __result.def.defName));

                if (__result == null && getter.RaceProps.Humanlike)
                {
                    //Log.Error("Patch_FoodUtility_BestFoodSourceOnMap.");

                    bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
                    if (!getterCanManipulate && getter != eater)
                    {
                        //Log.Error(getter.ToString() + " tried to find food to bring to " + (object)eater + " but " + (object)getter + " is incapable of Manipulation.", false);
                        return;
                    }
                    FoodPreferability minPref = minPrefOverride != FoodPreferability.Undefined ? minPrefOverride : (!eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory >= HungerCategory.UrgentlyHungry ? FoodPreferability.RawBad : FoodPreferability.MealAwful) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition);
                    Predicate<Thing> foodValidator = (Predicate<Thing>)(t =>
                    {
                        Building_StewPot StewPot = t as Building_StewPot;
                        if (StewPot != null)
                        {
                            if (!allowDispenserFull || !getterCanManipulate || (AOBADefOf.FT_MealStew.ingestible.preferability < minPref || AOBADefOf.FT_MealStew.ingestible.preferability > maxPref) || (!eater.WillEat(AOBADefOf.FT_MealStew, getter, true) || t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter) || !StewPot.refuelableComp.HasFuel || (!allowDispenserEmpty && !StewPot.HasEnoughFeedstockInHoppers() || (!t.InteractionCell.Standable(t.Map)))) || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                                return false;
                        }
                        return true;
                    });
                    //ThingRequest thingRequest = !((uint)(eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) > 0U & allowPlant) ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
                    Thing bestThing = null;
                    if (getter.RaceProps.Humanlike)
                    {
                        bestThing = Harmony_Patch_StewPot.SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(ThingRequest.ForDef(AOBADefOf.FT_StewPot)), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, foodValidator);
                        //Log.Error("BESTTHING:" + bestThing == null ? "NULL BESTTHING" : bestThing.def.defName);
                        if (allowHarvest & getterCanManipulate)
                        {
                            int searchRegionsMax = !forceScanWholeMap || bestThing != null ? 30 : -1;
                            Thing foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Predicate<Thing>)(x =>
                            {
                                return false;
                            }), (IEnumerable<Thing>)null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
                            //Log.Error("FOODSOURCE:" + foodSource == null ? "NULL BESTTHING" : foodSource.def.defName);
                            if (foodSource != null)
                            {
                                bestThing = foodSource;
                                foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                            }
                        }
                        if (foodDef == null && bestThing != null)
                            foodDef = FoodUtility.GetFinalIngestibleDef(bestThing, false);
                    }
                    __result = bestThing;
                }
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("GetMaxAmountToPickup")]
        static class Patch_FoodUtility_GetMaxAmountToPickup
        {
            static void Postfix(ref int __result, Thing food, Pawn pawn, int wantedCount)
            {
                //Log.Error("Patch_FoodUtility_GetMaxAmountToPickup");

                if (food is Building_StewPot)
                    __result = !pawn.CanReserve((LocalTargetInfo)food, 1, -1, (ReservationLayerDef)null, false) ? 0 : -1;
            }
        }

        [HarmonyPatch(typeof(RimWorld.FoodUtility))]
        [HarmonyPatch("ThoughtsFromIngesting")]
        static class Patch_FoodUtility_ThoughtsFromIngesting
        {
            static void Postfix(
              ref List<ThoughtDef> __result,
              Pawn ingester,
              Thing foodSource,
              ThingDef foodDef)
            {
                //Log.Error("Patch_FoodUtility_ThoughtsFromIngesting");

                if (ingester.needs == null || ingester.needs.mood == null || !ingester.RaceProps.Humanlike)
                    return;
                Building_StewPot StewPot = foodSource as Building_StewPot;
                if (StewPot != null)
                {
                    Thing feedInAnyHopper = StewPot.FindFeedInAnyHopper();
                    if (feedInAnyHopper != null)
                        Harmony_Patch_StewPot.AddIngestThoughtsFromIngredient(feedInAnyHopper.def, ingester, __result);
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_Ingest))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_Ingest_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_StewPot) || __instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_MealStew)))
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, AOBADefOf.FT_MealStew.label, (object)AOBADefOf.FT_MealStew, "", "", "", "");
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_FoodDeliver))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_FoodDeliver_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_StewPot) || __instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_MealStew)) && __instance.job.targetB != null && __instance.job.targetB.Thing != null)
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, AOBADefOf.FT_MealStew.label, (object)AOBADefOf.FT_MealStew, __instance.job.targetB.Thing.LabelShort, (object)__instance.job.targetB.Thing, "", (object)"");
                }
                return;
            }
        }

        [HarmonyPatch(typeof(RimWorld.JobDriver_FoodFeedPatient))]
        [HarmonyPatch("GetReport")]
        static class Patch_JobDriver_FoodFeedPatient_GetReport
        {
            static void Postfix(
              JobDriver_Ingest __instance,
              ref string __result)
            {
                if (__instance.job.targetA.Thing != null && (__instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_StewPot) || __instance.job.targetA.Thing.def.Equals(AOBADefOf.FT_MealStew)) && __instance.job.targetB != null && __instance.job.targetB.Thing != null)
                {
                    __result = JobUtility.GetResolvedJobReportRaw(__instance.job.def.reportString, AOBADefOf.FT_MealStew.label, (object)AOBADefOf.FT_MealStew, __instance.job.targetB.Thing.LabelShort, (object)__instance.job.targetB.Thing, "", (object)"");
                }
                return;
            }
        }

        // UTILS: pulled from core source because they're private there.
        private static Thing SpawnedFoodSearchInnerScan(
          Pawn eater,
          IntVec3 root,
          List<Thing> searchSet,
          PathEndMode peMode,
          TraverseParms traverseParams,
          float maxDistance = 9999f,
          Predicate<Thing> validator = null)
        {
            if (searchSet == null)
                return (Thing)null;
            Pawn pawn = traverseParams.pawn ?? eater;
            int num1 = 0;
            int num2 = 0;
            Thing thing = (Thing)null;
            float num3 = float.MinValue;
            for (int index = 0; index < searchSet.Count; ++index)
            {
                Thing search = searchSet[index];
                ++num2;
                float lengthManhattan = (float)(root - search.Position).LengthManhattan;
                if ((double)lengthManhattan <= (double)maxDistance)
                {
                    float num4 = FoodUtility.FoodOptimality(eater, search, FoodUtility.GetFinalIngestibleDef(search, false), lengthManhattan, false);
                    if ((double)num4 >= (double)num3 && pawn.Map.reachability.CanReach(root, (LocalTargetInfo)search, peMode, traverseParams) && search.Spawned && (validator == null || validator(search)))
                    {
                        thing = search;
                        num3 = num4;
                        ++num1;
                    }
                }
            }
            return thing;
        }
        private static void AddIngestThoughtsFromIngredient(
          ThingDef ingredient,
          Pawn ingester,
          List<ThoughtDef> ingestThoughts)
        {
            if (ingredient.ingestible == null)
                return;
            if (ingester.RaceProps.Humanlike && FoodUtility.IsHumanlikeMeat(ingredient))
            {
                ingestThoughts.Add(ingester.story.traits.HasTrait(TraitDefOf.Cannibal) ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient);
            }
            else
            {
                if (ingredient.ingestible.specialThoughtAsIngredient == null)
                    return;
                ingestThoughts.Add(ingredient.ingestible.specialThoughtAsIngredient);
            }
        }
    }
}
