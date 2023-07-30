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
    public class JobGiver_Guard : ThinkNode_JobGiver
    {
        private int recheckInterval = 120;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Guard obj = (JobGiver_Guard)base.DeepCopy(resolve);
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return null;
            }
            if (pawn.GetRegion() == null)
            {
                return null;
            }

            CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();
            if (memory == null || !memory.guardPosition.IsValid)
                return null;

            Job job = null;

            Thing target = this.FindAttackTarget(pawn);
            if (target == null)
                return null;

            // Melee attack if it is adjacent
            job = TryMeleeAttackTargetJob(pawn, target);

            // Shoot it if we can
            if (job == null)
                job = TryRangedAttackTargetJob(pawn, target);

            // Otherwise approach
            if (job == null)
                job = GoNearTarget(pawn, target);

            if (job != null)
            {
                job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Sprint, LocomotionUrgency.Walk);
            }

            return job;
        }

        private Thing FindAttackTarget(Pawn pawn)
        {
            float nearestSeenDistance = float.MaxValue;
            Thing target = null;
            
            foreach (IAttackTarget potentialTarget in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn))
            {
                if (!potentialTarget.ThreatDisabled(pawn) && AttackTargetFinder.IsAutoTargetable(potentialTarget))
                {
                    Thing potentialThing = (Thing)potentialTarget;
                    int distance = potentialThing.Position.DistanceToSquared(pawn.Position);
                    if ((float)distance < nearestSeenDistance && AttackTargetFinder.CanSee(pawn, potentialThing))
                    {
                        nearestSeenDistance = distance;
                        target = potentialThing;
                    }
                }
            }

            return target;
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
                        //newJob.killIncappedTarget = true;
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

            if (pawn.equipment.Primary != null && !pawn.equipment.Primary.def.IsMeleeWeapon && pawn.Position.DistanceTo(target.Position) < verb.verbProps.range)
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

        private Job GoNearTarget(Pawn pawn, Thing target)
        {
            Job newJob = JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_ApproachTarget, target);
            newJob.checkOverrideOnExpire = true;
            newJob.expiryInterval = recheckInterval;
            newJob.collideWithPawns = true;
            newJob.checkOverrideOnExpire = true;
            return newJob;
        }
    }
}