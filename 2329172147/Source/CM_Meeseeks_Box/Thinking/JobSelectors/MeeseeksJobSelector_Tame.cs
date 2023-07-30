using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace CM_Meeseeks_Box
{
    public class MeeseeksJobSelector_Tame : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.IsTaming;
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Job job = null;

            WorkGiver_Scanner scanner = savedJob.WorkGiverScanner;

            if (scanner != null && jobTarget != null && jobTarget.HasThing && !jobTarget.ThingDestroyed && jobTarget.Thing is Pawn)
            {
                Pawn targetPawn = jobTarget.Thing as Pawn;

                if (targetPawn.Faction == meeseeks.Faction || targetPawn.Dead)
                {
                    Logger.MessageFormat(this, "{0} already tamed or dead.", targetPawn);
                    jobAvailabilty = JobAvailability.Complete;
                }
                else if (targetPawn.RaceProps.EatsFood && !HasFoodToInteractAnimal(meeseeks, targetPawn))
                {
                    job = TakeFoodForAnimalInteractJob(meeseeks, targetPawn);
                }
                else if (TameUtility.TriedToTameTooRecently(targetPawn))
                {
                    jobAvailabilty = JobAvailability.Delayed;
                }
                else if (targetPawn.MapHeld != meeseeks.MapHeld)
                {
                    Logger.MessageFormat(this, "{0} not on map with {1}.", targetPawn);
                    job = ExitMapJob(meeseeks);
                }
                else if (targetPawn.Spawned)
                {
                    job = this.GetJobOnTarget(meeseeks, jobTarget, scanner);

                    if (job == null)
                        jobAvailabilty = JobAvailability.Delayed;
                }
                else
                {
                    Logger.WarningFormat(this, "Could not get handling job for {0}.", targetPawn);
                    jobAvailabilty = JobAvailability.Delayed;
                }
            }
            else
            {
                Logger.WarningFormat(this, "Unable to get scanner or target for job.");
            }

            if (job != null)
                jobAvailabilty = JobAvailability.Available;

            return job;
        }

        public override Job GetJobDelayed(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget)
        {
            Pawn targetPawn = jobTarget.Thing as Pawn;

            Job job = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_WaitNear, targetPawn);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            job.checkOverrideOnExpire = true;
            job.expiryInterval = 120;
            return job;
        }

        private bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
        {
            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            int num = 0;
            float num2 = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
            float num3 = 0f;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Thing thing = innerContainer[i];
                if (!tamee.WillEat(thing, pawn) || (int)thing.def.ingestible.preferability > 5 || thing.def.IsDrug)
                {
                    continue;
                }
                for (int j = 0; j < thing.stackCount; j++)
                {
                    num3 += thing.GetStatValue(StatDefOf.Nutrition);
                    if (num3 >= num2)
                    {
                        num++;
                        num3 = 0f;
                    }
                    if (num >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
        {
            FoodUtility.bestFoodSourceOnMap_minNutrition_NewTemp = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
            ThingDef foodDef;
            Thing thing = FoodUtility.BestFoodSourceOnMap(pawn, tamee, false, out foodDef, FoodPreferability.RawTasty, allowPlant: false, allowDrug: false, allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false);
            FoodUtility.bestFoodSourceOnMap_minNutrition_NewTemp = null;
            if (thing == null)
            {
                return null;
            }
            float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
            float nutrition = FoodUtility.GetNutrition(thing, foodDef);
            int count = Mathf.CeilToInt(num / nutrition);
            Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
            job.count = count;
            return job;
        }
    }
}
