using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Meeseeks_Box
{
    public class JobGiver_KillCreator : ThinkNode_JobGiver
    {
        private int recheckInterval = 120;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_KillCreator obj = (JobGiver_KillCreator)base.DeepCopy(resolve);
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            //if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            //{
            //    return null;
            //}
            //if (pawn.GetRegion() == null)
            //{
            //    return null;
            //}

            CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();
            if (memory == null)
                return null;

            //MentalState_MeeseeksKillCreator mentalState = pawn.MentalState as MentalState_MeeseeksKillCreator;
            //if (mentalState == null)
            //    return null;

            //Pawn creator = mentalState.target;

            Lord lord = pawn.GetLord();
            if (lord == null)
                return null;

            LordJob_MeeseeksKillCreator killCreatorLordJob = lord.LordJob as LordJob_MeeseeksKillCreator;
            if (killCreatorLordJob == null)
                return null;

            Pawn creator = killCreatorLordJob.Target;
            Thing target = creator.SpawnedParentOrMe;

            Job job = null;

            if (creator == null || creator.Dead || creator.Destroyed)
                job = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_EmbraceTheVoid);

            if (creator.MapHeld != pawn.MapHeld)
                job = ExitMap(pawn);

            if (job == null)
                job = TryMeleeAttackTargetJob(pawn, target);

            if (job == null)
                job = TryRangedAttackTargetJob(pawn, target);

            if (job == null)
                job = TryMeleeAttackAdjacentJob(pawn);

            if (job == null)
                job = TryGetEquipmentJob(pawn, target);

            if (job == null)
            {
                if (pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly, canBash: true))
                {
                    using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
                    {
                        if (!pawnPath.Found)
                        {
                            Logger.MessageFormat("Somehow no path was found to {0}", target);
                            return null;
                        }
                        IntVec3 cellBefore;
                        Thing blocker = pawnPath.FirstBlockingBuilding(out cellBefore, pawn);
                        if (blocker != null)
                        {
                            job = TryMeleeAttackTargetJob(pawn, blocker);
                            if (job == null)
                                job = TryRangedAttackTargetJob(pawn, blocker);
                            if (job == null)
                                job = GoToTarget(pawn, cellBefore);
                        }
                    }
                }
            }

            if (job == null)
                job = GoNearTarget(pawn, target);

            if (job != null)
            {
                job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Sprint, LocomotionUrgency.Walk);
                
            }

            return job;
        }

        private Job TryMeleeAttackTargetJob(Pawn pawn, Thing target)
        {
            Job newJob = null;

            for (int i = 0; i < 9; i++)
            {
                IntVec3 c = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(pawn.Map))
                {
                    continue;
                }
                List<Thing> thingList = c.GetThingList(pawn.Map);

                foreach (Thing thing in thingList)
                {
                    if (thing == target)
                    {
                        newJob = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                        newJob.maxNumMeleeAttacks = 1;
                        newJob.killIncappedTarget = true;
                        newJob.collideWithPawns = true;
                        newJob.expiryInterval = recheckInterval;
                        newJob.checkOverrideOnExpire = true;
                        return newJob;
                    }
                }
            }

            return null;
        }

        private Job TryRangedAttackTargetJob(Pawn pawn, Thing target)
        {
            Job newJob = null;

            Verb verb = pawn.CurrentEffectiveVerb;

            if (pawn.equipment.Primary != null && !pawn.equipment.Primary.def.IsMeleeWeapon && pawn.Position.DistanceTo(target.Position) < verb.verbProps.range && AttackTargetFinder.CanSee(pawn, target))
            {
                newJob = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
                newJob.maxNumStaticAttacks = 1;
                newJob.collideWithPawns = true;
                newJob.expiryInterval = recheckInterval;
                newJob.checkOverrideOnExpire = true;
                return newJob;
            }

            return null;
        }

        private Job TryMeleeAttackAdjacentJob(Pawn pawn)
        {
            Job newJob = null;

            for (int i = 0; i < 9; i++)
            {
                IntVec3 c = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(pawn.Map))
                {
                    continue;
                }
                List<Thing> thingList = c.GetThingList(pawn.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    Pawn target = thingList[j] as Pawn;
                    if (target != null && !target.Downed && target.HostileTo(pawn) && GenHostility.IsActiveThreatTo(target, pawn.Faction))
                    {
                        newJob = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                        newJob.maxNumMeleeAttacks = 1;
                        newJob.killIncappedTarget = false;
                        newJob.collideWithPawns = true;
                        newJob.expiryInterval = recheckInterval;
                        newJob.checkOverrideOnExpire = true;
                        return newJob;
                    }
                }
            }

            return null;
        }

        private Job TryGetEquipmentJob(Pawn pawn, Thing target)
        {
            Job newJob = null;
            Thing selectedEquipment = JobDriver_AcquireEquipment.FindEquipment(pawn);

            if (selectedEquipment != null && pawn.PositionHeld.DistanceTo(selectedEquipment.PositionHeld) < pawn.PositionHeld.DistanceTo(target.Position))
            {
                if (pawn.CanReach(selectedEquipment, PathEndMode.Touch, Danger.Deadly, canBash: true))
                {
                    using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, selectedEquipment, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)))
                    {
                        if (!pawnPath.Found)
                        {
                            Logger.MessageFormat("Somehow no path was found to {0}", selectedEquipment);
                            return null;
                        }
                        IntVec3 cellBefore;
                        Thing blocker = pawnPath.FirstBlockingBuilding(out cellBefore, pawn);
                        if (blocker != null)
                        {
                            newJob = TryMeleeAttackTargetJob(pawn, blocker);
                            if (newJob == null)
                                newJob = TryRangedAttackTargetJob(pawn, blocker);
                            if (newJob == null)
                                newJob = GoToTarget(pawn, cellBefore);
                        }
                        else
                        {
                            newJob = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_AcquireEquipment, selectedEquipment);
                        }
                    }
                }
            }

            return newJob;
        }

        private Job GoNearTarget(Pawn pawn, Thing target)
        {
            Job newJob = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_ApproachTarget, target);
            newJob.checkOverrideOnExpire = true;
            newJob.expiryInterval = recheckInterval;
            newJob.collideWithPawns = true;
            newJob.checkOverrideOnExpire = true;
            return newJob;
        }

        private Job GoToTarget(Pawn pawn, LocalTargetInfo target)
        {
            Job newJob = JobMaker.MakeJob(JobDefOf.Goto, target);
            newJob.checkOverrideOnExpire = true;
            newJob.expiryInterval = recheckInterval;
            newJob.collideWithPawns = true;
            newJob.checkOverrideOnExpire = true;
            return newJob;
        }

        private Job ExitMap(Pawn pawn)
        {
            bool canDig = !pawn.CanReachMapEdge();

            if (!TryFindGoodExitDest(pawn, canDig, out var dest))
            {
                return null;
            }
            if (canDig)
            {
                using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, dest, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAllDestroyableThings)))
                {
                    IntVec3 cellBefore;
                    Thing thing = path.FirstBlockingBuilding(out cellBefore, pawn);
                    if (thing != null)
                    {
                        Job job = DigUtility.PassBlockerJob(pawn, thing, cellBefore, canMineMineables: true, canMineNonMineables: true);
                        if (job != null)
                        {
                            return job;
                        }
                    }
                }
            }
            Job job2 = JobMaker.MakeJob(JobDefOf.Goto, dest);
            job2.exitMapOnArrival = true;
            job2.failIfCantJoinOrCreateCaravan = false;
            job2.expiryInterval = recheckInterval;
            job2.canBash = true;
            return job2;
        }

        private bool TryFindGoodExitDest(Pawn pawn, bool canDig, out IntVec3 spot)
        {
            TraverseMode mode = (canDig ? TraverseMode.PassAllDestroyableThings : TraverseMode.ByPawn);
            return RCellFinder.TryFindBestExitSpot(pawn, out spot, mode);
        }
    }
}