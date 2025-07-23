﻿using HarmonyLib;
using ProjectRimFactory.Storage;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ProjectRimFactory.Common.HarmonyPatches
{

    /// <summary>
    /// Patch for the Building_AdvancedStorageUnitIOPort
    /// Pawns starting Jobs check the IO Port for Items
    /// This affects mostly Bills on Workbenches
    /// </summary>
    [HarmonyPatch(typeof(Verse.AI.Pawn_JobTracker), "StartJob")]
    class Patch_Pawn_JobTracker_StartJob
    {
        
        /// <summary>
        /// Returns the Target Position of a Job
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="isHaulJobType"></param>
        /// <param name="newJob"></param>
        /// <param name="pawnPosition"></param>
        /// <returns></returns>
        private static bool TryGetTargetPos(ref IntVec3 targetPos, bool isHaulJobType, Job newJob, IntVec3 pawnPosition)
        {
            if (isHaulJobType)
            {
                //Haul Type Job
                targetPos = newJob.targetB.Thing?.Position ?? newJob.targetB.Cell;
                if (targetPos == IntVec3.Invalid) targetPos = pawnPosition;
                if (newJob.targetA == null) return false;

            }
            else
            {
                //Bill Type Job
                targetPos = newJob.targetA.Thing?.Position ?? newJob.targetA.Cell;
                if (newJob.targetB == IntVec3.Invalid && (newJob.targetQueueB == null || newJob.targetQueueB.Count == 0)) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Returns a list of LocalTargetInfos via <para>TargetItems</para> depending on the Job type
        /// </summary>
        /// <param name="TargetItems"></param>
        /// <param name="isHaulJobType"></param>
        /// <param name="newJob"></param>
        private static void GetTargetItems(ref List<LocalTargetInfo> TargetItems, bool isHaulJobType, Job newJob)
        {
            if (isHaulJobType)
            {
                TargetItems = new List<LocalTargetInfo>() { newJob.targetA };
            }
            else
            {
                if (newJob.targetQueueB == null || newJob.targetQueueB.Count == 0)
                {
                    TargetItems = new List<LocalTargetInfo>() { newJob.targetB };
                }
                else
                {
                    TargetItems = newJob.targetQueueB;
                }
            }
        }


        public static bool Prefix(Job newJob, ref Pawn ___pawn, JobCondition lastJobEndCondition = JobCondition.None,
            ThinkNode jobGiver = null, bool resumeCurJobAfterwards = false, bool cancelBusyStances = true
        , ThinkTreeDef thinkTree = null, JobTag? tag = null, bool fromQueue = false, bool canReturnCurJobToPool = false)
        {
            // No random moths eating my cloths
            if (___pawn?.Faction == null || !___pawn.Faction.IsPlayer) return true;
            // PickUpAndHaul "Compatibility" (by not messing with it)
            if (newJob.def.defName == "HaulToInventory") return true;
            
            // Cache Variables for Performance
            var pawnMap = ___pawn.Map;
            var pawnPos = ___pawn.Position;
            var prfMapComponent = PatchStorageUtil.GetPRFMapComponent(pawnMap);

            
            // This is the Position where we need the Item to be at
            IntVec3 targetPos = IntVec3.Invalid;
            // Check if this is a Haul Job
            bool usHaulJobType = newJob.targetA.Thing?.def?.category == ThingCategory.Item;
            
            // Get Target Position, Exit on fail
            if (!TryGetTargetPos(ref targetPos, usHaulJobType, newJob, pawnPos)) return true;

            List<KeyValuePair<float, Building_AdvancedStorageUnitIOPort>> Ports = 
                AdvancedIO_PatchHelper.GetOrderdAdvancedIOPorts(pawnMap, pawnPos, targetPos);
            List<LocalTargetInfo> TargetItems = null;
            GetTargetItems(ref TargetItems, usHaulJobType, newJob);
            foreach (var target in TargetItems)
            {
                if (target.Thing == null)
                {
                    //Log.Error($"ProjectRimfactory - Patch_Pawn_JobTracker_StartJob - Null Thing as Target: {target} - pawn:{___pawn} - Job:{newJob}");
                    continue;
                }

                var DistanceToTarget = AdvancedIO_PatchHelper.CalculatePath(pawnPos, target.Cell, targetPos);

                //Quick check if the Item could be in a DSU
                //Might have false Positives They are then filterd by AdvancedIO_PatchHelper.CanMoveItem
                //But should not have false Negatives
                if (prfMapComponent.ShouldHideItemsAtPos(target.Cell))
                {
                    foreach (var port in Ports)
                    {
                        var PortIsCloser = port.Key < DistanceToTarget;
                        if (PortIsCloser || (ConditionalPatchHelper.Patch_Reachability_CanReach.Status 
                                             && pawnMap.reachability.CanReach(pawnPos, 
                                                 target.Thing, Verse.AI.PathEndMode.Touch, 
                                                 TraverseParms.For(___pawn)) 
                                             && Patch_Reachability_CanReach.CanReachThing(target.Thing)))
                        {
                            if (AdvancedIO_PatchHelper.CanMoveItem(port.Value, target.Cell))
                            {
                                port.Value.AddItemToQueue(target.Thing);
                                port.Value.updateQueue();
                                
                                break;
                            }
                        }
                        else
                        {
                            //Since we use a orderd List we know
                            //if one ins further, the same is true for the rest
                            break;
                        }
                    }
                }
            }
            return true;
        }
    }
}
