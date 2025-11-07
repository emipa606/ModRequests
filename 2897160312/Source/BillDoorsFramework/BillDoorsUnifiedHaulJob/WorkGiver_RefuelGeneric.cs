using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Unity.Jobs;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsUnifiedHaulJob
{
    public class WorkGiver_RefuelGeneric : WorkGiver_Scanner
    {
        protected static Dictionary<Map, MapComponent_UnifiedHaulTracker> cachedMapComp = new Dictionary<Map, MapComponent_UnifiedHaulTracker>();

        protected MapComponent_UnifiedHaulTracker GetMapCompFor(Map map)
        {
            if (!cachedMapComp.ContainsKey(map))
            {
                cachedMapComp.Add(map, map.GetComponent<MapComponent_UnifiedHaulTracker>());
            }
            return cachedMapComp[map];
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => GetMapCompFor(pawn.Map).ActiveRefuelables;
        protected virtual JobDef RefuelJob => JobDefOf.BDF_Job_FillBuildingGeneric;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (GetMapCompFor(pawn.Map).RefuelablesReadOnly.Empty()) return true;
            return base.ShouldSkip(pawn, forced);
        }

        IRefuelable refuelableCache;
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsBurning() || t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced)) return false;

            foreach (var r in GetMapCompFor(pawn.Map).ActiveIRefuelableInThing(t))
            {
                if (forced ? r.RequestAmount <= 0 : !r.ShouldRefuelNow) continue;
                if (pawn.CanReserveIfuel(r))
                {
                    refuelableCache = r;
                    return true;
                }
                //Findfuel 已经检测了是否可预约
                else if ((r.FoundFuel = FindFuel(r, pawn)) != null)
                {
                    refuelableCache = r;
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            IRefuelable refuelable = refuelableCache.Parent == t ? refuelableCache : GetMapCompFor(pawn.Map).ActiveIRefuelableInThingWithFoundFuel(t).RandomElement();
            if (refuelable == null)
            {
                Log.Warning($"IRefuelable not found for {t} despite passing HasJobOnThing");
                return null;
            }
            Thing fuel = pawn.CanReserveIfuel(refuelable) ? refuelable.FoundFuel : FindFuel(refuelable, pawn);
            if (fuel == null)
            {
                Log.Warning($"fuel not found for {t} with an amount of {refuelable.RequestAmount} despite passing HasJobOnThing");
                return null;
            }
            Job j = JobMaker.MakeJob(RefuelJob, t, fuel);
            j.count = refuelable.RequestAmount;
            PawnIRefuelableCache.dict.SetOrAdd(pawn, refuelable);
            return j;
        }

        public Thing FindFuel(IRefuelable refuelable, Pawn pawn)
        {
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, refuelable.CurrentRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.None), refuelable.SearchRadius,
                    delegate (Thing f)
                    {
                        if (f.IsForbidden(pawn)) return false;
                        if (pawn.CanReserveIfuel(f, refuelable))
                        {
                            return refuelable.FuelValidator(f);
                        }
                        return false;
                    });
        }
    }
}
