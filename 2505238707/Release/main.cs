using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;


namespace _AFLD_AnimalsPreferGrazing
{
    [StaticConstructorOnStartup]
    static class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.afld.animalsprefergrazing");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FoodUtility))]
        [HarmonyPatch(nameof(FoodUtility.TryFindBestFoodSourceFor))]
        static class FoodUtility_TryFindBestFoodSourceFor_Patch
        {
            static bool Prefix(ref bool __result, Pawn getter, Pawn eater, /*FoodPreferability minPrefOverride,*/
                bool desperate, bool forceScanWholeMap, bool ignoreReservations, ref Thing foodSource, ref ThingDef foodDef, bool allowForbidden = false)
            {
// Log.Message("Running food prefix");

                if (getter == eater && eater.RaceProps.Animal && eater.RaceProps.Eats(FoodTypeFlags.Plant))
                {
                    //Log.Message("Animal");
                    HashSet<Thing> reserved_plant_filter = new HashSet<Thing>();
                    foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true))
                    {
                        Pawn pawn = item as Pawn;
                        if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(Verse.AI.TargetIndex.A).HasThing)
                        {
                            reserved_plant_filter.Add(pawn.CurJob.GetTarget(Verse.AI.TargetIndex.A).Thing);
                        }
                    }
                    Predicate<Thing> foodValidator = delegate (Thing t)
                    {
                        int stackCount = 1;
                        
                        if (FoodUtility.bestFoodSourceOnMap_minNutrition_NewTemp.HasValue)
                        {
                            float statValue = t.GetStatValue(StatDefOf.Nutrition);
                            stackCount = FoodUtility.StackCountForNutrition(FoodUtility.bestFoodSourceOnMap_minNutrition_NewTemp.Value, statValue);
                            if ((int)t.def.ingestible.preferability < (int)FoodPreferability.NeverForNutrition
                                || (int)t.def.ingestible.preferability > (int)FoodPreferability.MealLavish || !eater.WillEat(t, getter)
                                || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || t is Corpse
                                || (!allowForbidden && t.IsForbidden(getter))
                                || (!desperate && t.IsNotFresh()) || t.IsDessicated() || (!Verse.AI.PawnLocalAwareness.AnimalAwareOf(getter, t) && !forceScanWholeMap)
                                || (!ignoreReservations && !Verse.AI.ReservationUtility.CanReserve(getter, t, 10, stackCount)))
                            {
                                return false;
                            }
                        }
                        return true;
                    };
                    Predicate<Thing> plant_validator = delegate (Thing t)
                    {
                        if (!foodValidator(t))
                        {
                            return false;
                        }

                        Plant plant = t as Plant;
                        if (plant == null)
                        {
                            return false;
                        }

                        if (plant.def.plant.IsTree && !eater.RaceProps.Eats(FoodTypeFlags.Tree))
                        {
                            return false;
                        }

                        if (reserved_plant_filter.Contains(t))
                        {
                            return false;
                        }

                        if (!t.IngestibleNow)
                        {
                            return false;
                        }

                        if(!ForbidUtility.InAllowedArea(t.Position, getter)){
                            return false;
                        }

                        return Verse.AI.ReservationUtility.CanReserve(getter, t) ? true : false;
                    };
                    bool ignoreEntirelyForbiddenRegions = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, cellTarget: true) && getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
                    int maxRegionsToScan = 30;
                    if (forceScanWholeMap)
                    {
                        maxRegionsToScan = -1;
                    }
                    else if (getter.Faction == Faction.OfPlayer)
                    {
                        maxRegionsToScan = 100;
                    }
                    Thing bestPlant = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSource), Verse.AI.PathEndMode.OnCell, Verse.TraverseParms.For(getter), 9999f, plant_validator, null,
                        0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                    if(bestPlant != null)
                    {
                        foodSource = bestPlant;
                        foodDef = FoodUtility.GetFinalIngestibleDef(bestPlant);

                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

    }
}
