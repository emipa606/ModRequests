using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CM_Tradeable_Trinkets
{
    public class JoyGiver_PlayWithTrinket : JoyGiver
    {
        private static List<Thing> tmpCandidates = new List<Thing>();

        public override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobInternal(pawn, null);
        }

        public override Job TryGiveJobInGatheringArea(Pawn pawn, IntVec3 gatheringSpot, float maxRadius = -1f)
        {
            return TryGiveJobInternal(pawn, (Thing x) => !x.Spawned || (GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map) && (maxRadius < 0f || x.Position.InHorDistOf(gatheringSpot, maxRadius))));
        }

        private Job TryGiveJobInternal(Pawn pawn, Predicate<Thing> extraValidator)
        {
            Thing thing = BestTrinket(pawn, extraValidator);
            if (thing != null)
            {
                return CreateJob(thing, pawn);
            }
            return null;
        }

        protected virtual Thing BestTrinket(Pawn pawn, Predicate<Thing> extraValidator)
        {
            Predicate<Thing> predicate = delegate (Thing t)
            {
                if (t.Spawned)
                {
                    if (!pawn.CanReserve(t))
                    {
                        return false;
                    }
                    if (t.IsForbidden(pawn))
                    {
                        return false;
                    }
                    if (!t.IsSociallyProper(pawn))
                    {
                        return false;
                    }
                    if (!t.IsPoliticallyProper(pawn))
                    {
                        return false;
                    }
                }
                return (extraValidator == null || extraValidator(t)) ? true : false;
            };

            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (SearchSetWouldInclude(innerContainer[i]) && predicate(innerContainer[i]))
                {
                    return innerContainer[i];
                }
            }
            tmpCandidates.Clear();
            GetSearchSet(pawn, tmpCandidates);
            if (tmpCandidates.Count == 0)
            {
                return null;
            }
            Thing result = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, tmpCandidates, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, predicate);
            tmpCandidates.Clear();
            return result;
        }

        protected virtual bool SearchSetWouldInclude(Thing thing)
        {
            if (def.thingDefs == null)
            {
                return false;
            }
            return def.thingDefs.Contains(thing.def);
        }

        protected virtual Job CreateJob(Thing trinket, Pawn pawn)
        {
            Job job = JobMaker.MakeJob(TrinketsModDefOf.CM_Tradeable_Trinkets_Job_PlayWithTrinket, trinket);
            job.count = 1;
            return job;
        }
    }

}

