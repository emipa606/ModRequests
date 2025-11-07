using System;
using Verse;
using Verse.AI;
using UnityEngine;
using RimWorld;

namespace BillDoorsUnifiedHaulJob
{
    public static class IRefuelableUtil
    {
        public static void SpawnRegister(IRefuelable refuelable)
        {
            if (refuelable.Parent == null || !refuelable.Parent.SpawnedOrAnyParentSpawned) return;
            SpawnRegister(refuelable.Parent.MapHeld, refuelable);
        }

        public static void SpawnRegister(Map map, IRefuelable refuelable)
        {
            map.GetComponent<MapComponent_UnifiedHaulTracker>()?.Register(refuelable);
        }


        public static void Deregister(Map map, IRefuelable refuelable)
        {
            map.GetComponent<MapComponent_UnifiedHaulTracker>()?.Deregister(refuelable);
        }

        public static IRefuelable ActiveIRefuelableInThingRefuelableFrom(Thing thing, Thing fuel, int amount)
        {
            if (thing.MapHeld != null) return thing.MapHeld.GetComponent<MapComponent_UnifiedHaulTracker>().ActiveIRefuelableInThingRefuelableFrom(thing, fuel, amount);

            if (thing is IRefuelable refuelable && refuelable.RequestAmount >= amount && refuelable.FuelValidator(fuel))
            {
                return refuelable;
            }
            if (thing is ThingWithComps twc)
            {
                foreach (var i in twc.AllComps)
                {
                    if (i is IRefuelable r)
                    {
                        if (r.RequestAmount >= amount && r.FuelValidator(fuel)) return r;
                    }
                }
            }
            return null;
        }

        public static bool CanReserveIfuel(this Pawn pawn, Thing fuel, IRefuelable refuelable)
        {
            return pawn.CanReserve(fuel, 10, Math.Min(refuelable.RequestAmount, fuel.stackCount));
        }

        public static bool CanReserveIfuel(this Pawn pawn, IRefuelable refuelable)
        {
            if (refuelable.FoundFuel == null || refuelable.FoundFuel.IsForbidden(pawn)) return false;
            return pawn.CanReserveIfuel(refuelable.FoundFuel, refuelable);
        }

        public static void DropOffThing(int num, ThingDef thingDef, Map map, IntVec3 position)
        {
            while (num > 0)
            {
                Thing thing = ThingMaker.MakeThing(thingDef);
                thing.stackCount = Mathf.Min(num, thingDef.stackLimit);
                num -= thing.stackCount;
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near);
            }
        }
    }
}
