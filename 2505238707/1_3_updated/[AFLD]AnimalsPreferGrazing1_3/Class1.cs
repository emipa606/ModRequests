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
        [HarmonyPatch(nameof(FoodUtility.TryFindBestFoodSourceFor_NewTemp))]
        static class FoodUtility_TryFindBestFoodSourceFor_NewTempPatch
        {
            static bool Prefix(ref bool __result, Pawn getter, Pawn eater, /*FoodPreferability minPrefOverride,*/
                bool desperate, bool forceScanWholeMap, bool ignoreReservations, ref Thing foodSource, ref ThingDef foodDef, bool allowForbidden = false, bool calculateWantedStackCount = false)
            {
                // Log.Message("Running food prefix");
                bool is_plant_eater = eater.RaceProps.Eats(FoodTypeFlags.Plant);
                bool is_corpse_eater = eater.RaceProps.Eats(FoodTypeFlags.Corpse);
                if (getter == eater && eater.RaceProps.Animal && (is_plant_eater || is_corpse_eater) )
                {
                    //Log.Message("Animal");
                    HashSet<Thing> reserved_plant_or_corpse_filter = new HashSet<Thing>();

                    foreach (Thing item in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, useCenter: true))
                    {
                        Pawn pawn = item as Pawn;
                        if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Ingest && pawn.CurJob.GetTarget(Verse.AI.TargetIndex.A).HasThing)
                        {
                            Thing thing = pawn.CurJob.GetTarget(Verse.AI.TargetIndex.A).Thing;
                            reserved_plant_or_corpse_filter.Add(thing);
                        }
                    }
                    Predicate<Thing> food_validator = delegate (Thing t)
                    {
                        int stack_count = 1;
                        float stat_value = t.GetStatValue(StatDefOf.Nutrition);
                        
                        if (calculateWantedStackCount)
                        {
                            stack_count = FoodUtility.WillIngestStackCountOf(eater, t.def, stat_value);
                        }
                        if (!eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible || !t.IngestibleNow || (!Verse.AI.PawnLocalAwareness.AnimalAwareOf(getter, t) && !forceScanWholeMap) || (!ignoreReservations && !Verse.AI.ReservationUtility.CanReserve(getter, t, 10, stack_count)))
                        {
                            return false;
                        }
                        return true;
                    };
                    Predicate<Thing> misc_validator = delegate (Thing t)
                    {
                        if (!food_validator(t))
                        {
                            return false;
                        }

                        if (reserved_plant_or_corpse_filter.Contains(t))
                        {
                            return false;
                        }

                        if (!t.IngestibleNow)
                        {
                            return false;
                        }

                        if (!ForbidUtility.InAllowedArea(t.Position, getter))
                        {
                            return false;
                        }

                        return Verse.AI.ReservationUtility.CanReserve(getter, t);
                    };

                    Predicate<Thing> corpse_validator = delegate (Thing t) { 
                        bool corpse = t is Corpse;
                        if (corpse) return misc_validator(t);
                        return false;
                    };
                    Predicate<Thing> plant_validator = delegate (Thing t)
                    {
                        bool plant = t is Plant;
                        if (plant) return misc_validator(t);
                        return false;
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

                    if (is_corpse_eater)
                    {
                        Thing bestCorpse = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSource), Verse.AI.PathEndMode.OnCell, Verse.TraverseParms.For(getter), 9999f, corpse_validator, null,
                            0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                        if (bestCorpse != null)
                        {
                            foodSource = bestCorpse;
                            foodDef = FoodUtility.GetFinalIngestibleDef(bestCorpse);

                            __result = true;
                            return false;
                        }
                    }
                    if (is_plant_eater)
                    {
                        Thing bestPlant = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSource), Verse.AI.PathEndMode.OnCell, Verse.TraverseParms.For(getter), 9999f, plant_validator, null,
                            0, maxRegionsToScan, forceAllowGlobalSearch: false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                        if (bestPlant != null)
                        {
                            foodSource = bestPlant;
                            foodDef = FoodUtility.GetFinalIngestibleDef(bestPlant);

                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

    }
}