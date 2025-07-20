using System;
using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_No_Pawn_Left_Behind
{
    public class JobGiver_Rescue : ThinkNode_JobGiver
    {
        protected float rescueChance = 0.33f;
        protected float searchRadius = 10f;

        protected float opinionPriorityWeight = -1.0f;
        protected float marketValuePriorityWeight = -1.0f;
        protected float distancePriorityWeight = -1.0f;

        protected float healthSearchDistanceWeight = -1.0f;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Rescue obj = (JobGiver_Rescue)base.DeepCopy(resolve);
            obj.rescueChance = rescueChance;
            obj.searchRadius = searchRadius;
            obj.opinionPriorityWeight = opinionPriorityWeight;
            obj.marketValuePriorityWeight = marketValuePriorityWeight;
            obj.distancePriorityWeight = distancePriorityWeight;
            obj.healthSearchDistanceWeight = healthSearchDistanceWeight;
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            float random = Rand.Value;

            float finalRescueChance = rescueChance;
            if (RescueMod.settings.rescueChanceOverride)
                finalRescueChance = RescueMod.settings.rescueChance;

            if (Prefs.DevMode && RescueMod.settings.logPriorities)
                Log.Message(String.Format("{0} - checking rescue chance {1}/{2}", pawn, random, finalRescueChance));

            if (random > finalRescueChance)
                return null;

            if (pawn.AnimalOrWildMan() || pawn.Faction == Faction.OfPlayer || !RCellFinder.TryFindBestExitSpot(pawn, out var exitSpot))
                return null;

            if (TryFindRescuableAlly(pawn, out var ally))
            {
                Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
                job.targetA = ally;
                job.targetB = exitSpot;
                job.count = 1;
                return job;
            }
            return null;
        }

        private bool TryFindRescuableAlly(Pawn rescuer, out Pawn ally, List<Thing> disallowed = null)
        {
            if (!rescuer.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !rescuer.Map.reachability.CanReachMapEdge(rescuer.Position, TraverseParms.For(rescuer, Danger.Some)))
            {
                ally = null;
                return false;
            }

            // Multiply search radius by health to make wounded people a little less bold in the face of danger
            float maxDist = searchRadius;
            if (RescueMod.settings.searchRadiusOverride)
                maxDist = RescueMod.settings.searchRadius;
            if (healthSearchDistanceWeight > 0.0f)
                maxDist *= (healthSearchDistanceWeight * rescuer.health.summaryHealth.SummaryHealthPercent);

            float maxDistSquared = maxDist * maxDist;

            if (Prefs.DevMode && RescueMod.settings.logPriorities)
                Log.Message(String.Format("{0} - searching for pawn to rescue within {1}", rescuer, maxDist));

            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn = t as Pawn;
                if (pawn == null || pawn == rescuer || !pawn.RaceProps.Humanlike || !pawn.Downed || pawn.Faction != rescuer.Faction || !rescuer.CanReserve(pawn))
                    return false;
                return (disallowed == null || !disallowed.Contains(pawn)) && rescuer.Position.DistanceToSquared(pawn.Position) < maxDistSquared;
            };

            Func<Thing, float> evaluator = delegate (Thing t)
            {
                Pawn pawn = t as Pawn;

                if (!validator(t))
                    return -1.0f;

                float priority = 1.0f;

                if (opinionPriorityWeight > 0.0f)
                {
                    float opinionValue = (rescuer.relations.OpinionOf(pawn) + 100.0f) / 200.0f;
                    opinionValue *= opinionValue;

                    priority *= (opinionPriorityWeight * opinionValue);
                }

                if (marketValuePriorityWeight > 0.0f)
                {
                    float marketValue = Mathf.Clamp(pawn.MarketValue, 200.0f, 3000.0f);
                    priority *= (marketValuePriorityWeight * marketValue);
                }

                if (distancePriorityWeight > 0.0f)
                    priority /= (distancePriorityWeight * (rescuer.Position.DistanceTo(pawn.Position) + 1.0f));

                if (Prefs.DevMode && RescueMod.settings.logPriorities)
                    Log.Message(String.Format("{0} - priority: {1}, opinion: {2}, marketValue: {3}, distance: {4}", pawn, priority, rescuer.relations.OpinionOf(pawn), pawn.MarketValue, (rescuer.Position.DistanceTo(pawn.Position) + 1.0f)));

                return priority;
            };

            //ally = (Pawn)GenClosest.ClosestThingReachable(rescuer.Position, rescuer.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), maxDist, validator);
            ally = (Pawn)GenClosest.ClosestThing_Regionwise_ReachablePrioritized(rescuer.Position, rescuer.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), maxDist, validator, evaluator, 15, 15);

            if (Prefs.DevMode && RescueMod.settings.logPriorities && ally != null)
                Log.Message(String.Format("{0} - rescuing {1}", rescuer, ally));

            return ally != null;
        }
    }
}