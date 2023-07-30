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
    public class MeeseeksJobSelector_DoBill : MeeseeksJobSelector
    {
        public override bool UseForJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob)
        {
            return savedJob.IsDoBill;
        }

        public override Job GetJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Bill bill = jobTarget.bill;

            Logger.MessageFormat(this, "Checking for bill job...");

            if (bill == null)
                return ScanForJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);

            if (bill.deleted)
            {
                jobAvailabilty = JobAvailability.Complete;
                return null;
            }

            if (bill is Bill_Medical)
            {
                return GetMedicalBillJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);
            }
            else if (bill is Bill_Production)
            {
                return GetProductionBillJob(meeseeks, memory, savedJob, jobTarget, ref jobAvailabilty);
            }

            return null;
        }

        private Job GetMedicalBillJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Bill_Medical bill = jobTarget.bill as Bill_Medical;
            Job job = null;

            if (jobTarget != null && jobTarget.HasThing && !jobTarget.ThingDestroyed && jobTarget.Thing is Pawn && !(jobTarget.Thing as Pawn).Dead)
            {
                Pawn targetPawn = jobTarget.Thing as Pawn;
                if (targetPawn == null || targetPawn.Dead || !bill.CompletableEver)
                {
                    bill.deleted = true;
                    jobAvailabilty = JobAvailability.Complete;
                }
                else
                {
                    MeeseeksBillStorage billStorage = Current.Game.World.GetComponent<MeeseeksBillStorage>();
                    BillStack billStack = targetPawn.BillStack;

                    jobAvailabilty = JobAvailability.Delayed;

                    if (targetPawn.UsableForBillsAfterFueling() && meeseeks.CanReserve(targetPawn, 1, -1, null, true))
                    {
                        List<ThingCount> chosenIngredients = new List<ThingCount>();
                        // Screw you I need this function
                        bool result = (bool)typeof(WorkGiver_DoBill).GetMethod("TryFindBestBillIngredients", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { bill, meeseeks, targetPawn, chosenIngredients });

                        if (result)
                        {
                            Job haulOffJob;
                            job = WorkGiver_DoBill.TryStartNewDoBillJob(meeseeks, bill, targetPawn, chosenIngredients, out haulOffJob);
                            bill.billStack.billGiver = targetPawn as IBillGiver;
                        }

                        if (job == null)
                            jobAvailabilty = JobAvailability.Delayed;
                        else
                            jobAvailabilty = JobAvailability.Available;
                    }
                }
            }
            else
            {
                bill.deleted = true;
                jobAvailabilty = JobAvailability.Complete;
            }

            return job;
        }

        private Job GetProductionBillJob(Pawn meeseeks, CompMeeseeksMemory memory, SavedJob savedJob, SavedTargetInfo jobTarget, ref JobAvailability jobAvailabilty)
        {
            Bill_Production bill = jobTarget.bill as Bill_Production;
            Job job = null;

            Logger.MessageFormat(this, "Checking for production bill job...");

            if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount && bill.repeatCount < 1)
            {
                bill.deleted = true;
                jobAvailabilty = JobAvailability.Complete;
                return null;
            }
            else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                // Might be here without a billgiver (after a save?) so try to set the current target
                if (jobTarget.Thing != null)
                    bill.billStack.billGiver = jobTarget.Thing as IBillGiver;

                // Otherwise count them later I guess :P
                if (bill.Map != null)
                {
                    int productCount = bill.recipe.WorkerCounter.CountProducts(bill);
                    if (productCount >= bill.targetCount)
                    {
                        bill.deleted = true;
                        jobAvailabilty = JobAvailability.Complete;
                        return null;
                    }
                }
            }

            Logger.MessageFormat(this, "Checking workstations...");

            bool workStationValid = (jobTarget.HasThing && !jobTarget.ThingDestroyed &&
               meeseeks.CanReserve(jobTarget.Thing, 1, -1, null, false) &&
               ((jobTarget.Thing as IBillGiver).CurrentlyUsableForBills() || (jobTarget.Thing as IBillGiver).UsableForBillsAfterFueling()));

            if (!workStationValid)
            {
                Logger.MessageFormat(this, "We think the workstation is invalid looking for new one");
                List<Building> buildings = meeseeks.MapHeld.listerBuildings.allBuildingsColonist.Where(building => building is IBillGiver &&
                                                                                                       savedJob.workGiverDef.fixedBillGiverDefs.Contains(building.def) &&
                                                                                                       meeseeks.CanReserve(building, 1, -1, null, false) &&
                                                                                                       ((building as IBillGiver).CurrentlyUsableForBills() || (building as IBillGiver).UsableForBillsAfterFueling())).ToList();

                if (buildings.Count > 0)
                {
                    Logger.MessageFormat(this, "Found new one");

                    buildings.Sort((a, b) => (int)(meeseeks.PositionHeld.DistanceTo(a.Position) - meeseeks.PositionHeld.DistanceTo(b.Position)));
                    jobTarget.target = buildings[0];
                    workStationValid = true;
                }
                else
                {
                    Logger.MessageFormat(this, "Found no alternate workstations...");
                }
            }
            else
            {
                Logger.MessageFormat(this, "We think the workstation is valid...");
            }

            if (workStationValid)
            {
                IBillGiver billGiver = jobTarget.Thing as IBillGiver;
                bill.billStack.billGiver = billGiver;

                Bill_ProductionWithUft bill_ProductionWithUft = bill as Bill_ProductionWithUft;
                if (bill_ProductionWithUft != null)
                {
                    if (bill_ProductionWithUft.BoundUft != null)
                    {
                        if (bill_ProductionWithUft.BoundUft.Creator.kindDef == MeeseeksDefOf.MeeseeksKind && meeseeks.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly))
                        {
                            job = FinishUftJob(meeseeks, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft, billGiver);
                        }
                    }
                    else
                    {
                        Predicate<Thing> validator = (Thing t) => ((UnfinishedThing)t).Recipe == bill.recipe && ((UnfinishedThing)t).Creator.kindDef == MeeseeksDefOf.MeeseeksKind && ((UnfinishedThing)t).ingredients.TrueForAll((Thing x) => bill.IsFixedOrAllowedIngredient(x.def)) && meeseeks.CanReserve(t);
                        UnfinishedThing unfinishedThing = (UnfinishedThing)GenClosest.ClosestThingReachable(meeseeks.Position, meeseeks.Map, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell, TraverseParms.For(meeseeks), 9999f, validator);
                        if (unfinishedThing != null)
                        {
                            job = FinishUftJob(meeseeks, unfinishedThing, bill_ProductionWithUft, billGiver);
                        }
                    }
                }

                if (job == null)
                {
                    List<ThingCount> chosenIngredients = new List<ThingCount>();
                    // Screw you I need this function
                    bool result = (bool)typeof(WorkGiver_DoBill).GetMethod("TryFindBestBillIngredients", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { bill, meeseeks, jobTarget.Thing, chosenIngredients });

                    if (result)
                    {
                        Job haulOffJob = null;
                        job = WorkGiver_DoBill.TryStartNewDoBillJob(meeseeks, bill, billGiver, chosenIngredients, out haulOffJob);
                    }
                }

            }

            if (job == null)
                jobAvailabilty = JobAvailability.Delayed;
            else
                jobAvailabilty = JobAvailability.Available;

            return job;
        }

        private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill, IBillGiver billGiver)
        {
            if (uft.Creator != pawn)
            {
                //Log.Error(string.Concat("Tried to get FinishUftJob for ", pawn, " finishing ", uft, " but its creator is ", uft.Creator));
                return null;
            }
            Job job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, billGiver, uft);
            if (job != null && job.targetA.Thing != uft)
            {
                return job;
            }
            Job job2 = JobMaker.MakeJob(JobDefOf.DoBill, (Thing)billGiver);
            job2.bill = bill;
            job2.targetQueueB = new List<LocalTargetInfo> { uft };
            job2.countQueue = new List<int> { 1 };
            job2.haulMode = HaulMode.ToCellNonStorage;
            return job2;
        }
    }
}
