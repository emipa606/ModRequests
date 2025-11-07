using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Principal;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsUnifiedHaulJob
{
    public class JobDriver_RefuelGeneric : JobDriver
    {
        protected TargetIndex indexRefuelee = TargetIndex.A;
        protected TargetIndex indexFuel = TargetIndex.B;
        protected TargetIndex indexSpawnedParent = TargetIndex.C;

        protected Thing Refuelee => job.GetTarget(indexRefuelee).Thing;
        protected Thing SpawnedParentOrMe => Refuelee.SpawnedParentOrMe;
        protected Thing ThingFuel => job.GetTarget(indexFuel).Thing;

        IRefuelable refuelable;

        //正确的方式应该是给irefuelable加iloadreferenceable接口。但是现在晚了。1.6再说吧
        public IRefuelable Refuelable
        {
            get
            {
                if (refuelable == null)
                {
                    if (pawn != null && PawnIRefuelableCache.dict.ContainsKey(pawn))
                    {
                        refuelable = PawnIRefuelableCache.dict[pawn];
                        PawnIRefuelableCache.dict.Remove(pawn);
                    }
                    else
                    {
                        refuelable = IRefuelableUtil.ActiveIRefuelableInThingRefuelableFrom(Refuelee, ThingFuel, maxPossibleCount);
                    }
                }
                return refuelable;
            }
            set { refuelable = value; }
        }

        int maxPossibleCount => Mathf.Min(job.count, ThingFuel.stackCount, pawn.carryTracker.MaxStackSpaceEver(ThingFuel.def));

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(SpawnedParentOrMe, job, 1, -1, null, errorOnFailed) && pawn.Reserve(ThingFuel, job, 10, maxPossibleCount, null, errorOnFailed))
            {
                Refuelable.FoundFuel = null;
                return true;
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (Refuelable == null) Log.Error($"JobDriver_RefuelGeneric from {ThingFuel} to {Refuelee} with a count of {job.count} returned null ActiveIRefuelableInThingRefuelableFrom");
            AddFailCondition(() => Refuelable == null || Refuelable.RequestAmount <= 0);
            job.targetC = SpawnedParentOrMe;
            this.FailOnDestroyedNullOrForbidden(indexSpawnedParent);
            this.FailOnBurningImmobile(indexSpawnedParent);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = Refuelable.RequestAmount;
            });

            Toil reserveThing = Toils_Goto.GotoThing(indexFuel, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(indexFuel).FailOnSomeonePhysicallyInteracting(indexFuel);
            yield return reserveThing;
            yield return Toils_Haul.StartCarryThing(indexFuel, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, reserve: false).FailOnDestroyedNullOrForbidden(indexFuel);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveThing, indexFuel, TargetIndex.None, takeFromValidStorage: true);

            if (SpawnedParentOrMe != pawn) yield return Toils_Goto.GotoThing(indexSpawnedParent, PathEndMode.Touch);

            //读条
            if (Refuelable.RefuelWaitTick > 0)
            {
                Toil toilWait = Toils_General.Wait(100);
                toilWait.WithProgressBarToilDelay(indexSpawnedParent);
                yield return toilWait;
            }

            Toil addThing = ToilMaker.MakeToil();
            addThing.initAction = delegate ()
            {
                Refuelable.RefuelEffect(ThingFuel, pawn);
                Refuelable.FoundFuel = null;
            };
            addThing.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return addThing;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref refuelable, "IRefuelable");
        }
    }
}
