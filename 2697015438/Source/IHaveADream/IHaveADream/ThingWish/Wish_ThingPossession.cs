using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace HDream
{
    public abstract class Wish_ThingPossession<T> : Wish_Thing<T>
    {

        protected int baseCount = 0;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref actualCount, "thingCount", 0);
            Scribe_Values.Look(ref baseCount, "baseCount", 0);
            Scribe_Values.Look(ref doAtTick, "doAtTick", 0);
        }

        public override void PostMake()
        {
            base.PostMake();

            if (!Def.countPreWishProgress) baseCount = CountMatch();
            else if (CountMatch() >= def.amountNeeded) OnMakeFulfill();
            else
            {
                actualCount = CountMatch();
                if (Def.noMoodPreWishProgress) progressCount = Mathf.FloorToInt((actualCount / Def.amountNeeded) / (Def.progressStep));
            } 
        }

        public override void Tick()
        {
            base.Tick();
            TickToResolve();
        }

        protected abstract int ThingMatching(IEnumerable<Thing> things, T match);

        protected override abstract int CountMatch();

        protected override void CheckResolve()
        {
            base.CheckResolve();
            int newCount = CountMatch() - baseCount;
            CountProgressStep(ref actualCount, newCount);
            if (newCount >= def.amountNeeded) OnFulfill();
        }

        protected virtual int AdjustForSpecifiedCount(int count, int specificTotal)
        {
            if (Def.countAmountPerInfo)
            {
                if (count < specificTotal) return 0;
                return 1;
            }
            return count;
        }


        public override string DescriptionToFulfill
        {
            get
            {
                return base.DescriptionToFulfill + " (" + actualCount.ToString() + "/" + Def.amountNeeded.ToString() + ")";
            }
        }
    }
}
