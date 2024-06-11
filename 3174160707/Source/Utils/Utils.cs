using Reload.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Reload
{
    public class Utils
    {
        public static List<Thing> GenerateThings(ThingDef thingDef, int amount)
        {
            if (thingDef == null || amount <= 0)
                return null;

            List<Thing> result = new List<Thing>();
            int stackLimit = thingDef.stackLimit;

            while (amount > 0)
            {
                Thing thing = ThingMaker.MakeThing(thingDef);
                thing.stackCount = Mathf.Min(amount, stackLimit);
                amount -= stackLimit;
                result.Add(thing);
            }

            return result;
        }
        public static void GenerateThingsToGround(IntVec3 position, Map map, ThingDef thingDef, int amount)
        {
            if (map == null || thingDef == null || amount <= 0)
                return;

            foreach(Thing thing in GenerateThings(thingDef, amount))
            {
                GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near);
            }
        }
        public static void AddThingsToInventory(Pawn pawn, ThingDef thingDef, int amount)
        {
            if (pawn == null || thingDef == null || amount <= 0)
                return;

            foreach (Thing thing in GenerateThings(thingDef, amount))
            {
                pawn.inventory.innerContainer.TryAdd(thing);
            }
        }
        public static void FetchThings(Pawn pawn, ThingDef thingDef, int amount)
        {
            if (pawn == null || thingDef == null || amount <= 0)
                return;
            IntRange desiredQuantity = new IntRange(1, amount);
            List<Thing> chosenThings = RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, pawn.Position, desiredQuantity,
               x => x.def == thingDef);
            if (chosenThings.NullOrEmpty())
                return;

            int targetCount = Math.Min(chosenThings.Sum(x => x.stackCount), amount);
            Job job = JobMaker.MakeJob(ReloadJobDefOf.R_FetchThings, pawn);
            job.targetQueueB = (from t in chosenThings
                                select new LocalTargetInfo(t)).ToList();
            job.count = targetCount;

            pawn.jobs.TryTakeOrderedJob(job);
        }
        public static void FetchThings(Pawn pawn, List<Thing> targets, int amount)
        {
            if (pawn == null || targets.NullOrEmpty() || amount <= 0)
                return;

            targets.SortBy(x => x.def.BaseMass);

            Job job = JobMaker.MakeJob(ReloadJobDefOf.R_FetchThings, pawn);
            job.targetQueueB = (from t in targets
                                select new LocalTargetInfo(t)).ToList();
            job.count = amount;

            pawn.jobs.TryTakeOrderedJob(job);
            return;
        }
    }
}
