using RimWorld;
using HarmonyLib;
using Verse;
using System;
using UnityEngine;

namespace HDream
{
    public class WishComp_Timed : WishComp
    {

        public WishCompProperties_Timed Props => (WishCompProperties_Timed)props;
        protected float TickToHold => Props.daysToHold * GenDate.TicksPerDay;

        protected int holdingTime = 0;

        protected int thoughtTime = 0;

        protected bool conditionSatisfied = false;

        protected float pendingFloat = 0;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref thoughtTime, "thoughtTime", 0);
            Scribe_Values.Look(ref holdingTime, "holdingTime", 0);
            Scribe_Values.Look(ref pendingFloat, "pendingFloat", 0);
            Scribe_Values.Look(ref conditionSatisfied, "conditionSatisfied", false);
        }
        public override bool CompOnFulfill()
        {
            if(conditionSatisfied) return holdingTime >= TickToHold;
            conditionSatisfied = true;
            pendingFloat = parent.pendingTicks;

            int pendingToRemove = Mathf.FloorToInt(parent.pendingCount * Props.removePendingOnHoldPercent) + Props.removePendingOnHoldOffset;

            if (!parent.preventBuff) 
            { 
                if (thoughtTime > HDThoughtDefOf.WishOnHold.DurationTicks)
                {
                    parent.Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishOnHold, parent.TierIndex));
                    thoughtTime = 0;
                }
            }
            if (pendingToRemove >= parent.pendingCount) while (parent.pendingCount > 0) parent.RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref parent.pendingCount);
            else for (int i = 0; i < pendingToRemove; i++) parent.RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref parent.pendingCount);

            return holdingTime >= TickToHold;
        }

        public override bool CompOnPending(ref int pendingTick)
        {
            if (!conditionSatisfied) return true;
            if (pendingTick == 0) return false;
            pendingFloat -= 1 * Props.removePendingPerTickFactor;
            if (pendingFloat * (1 / Props.removePendingPerTickFactor) < (GenDate.TicksPerDay / parent.def.upsetPerDay) * (parent.pendingCount + 1))
            {
                parent.RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref parent.pendingCount);
            }
            pendingTick = (int)pendingFloat;
            return false;
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            thoughtTime = HDThoughtDefOf.WishOnHold.DurationTicks + 1;
        }

        public override void CompPostChange(int value)
        {
            base.CompPostChange(value);
            if (value < 0 && conditionSatisfied)
            {
                conditionSatisfied = false;
                if(Props.resetTimerOnFailHold) holdingTime = 0;
                if(!parent.preventDebuff) parent.Memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishUnHold, parent.TierIndex));
            }
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
            if (conditionSatisfied)
            {
                holdingTime++;
                if (holdingTime >= TickToHold) parent.OnFulfill();
            }
            thoughtTime++;
        }

        public override string CompDescription()
        {
            string desc = base.CompDescription() + TranslationKey.WISHCOMP_TIMED_DESCRIPTION.Translate(Props.daysToHold)+ "\n";
            desc += (conditionSatisfied) ? TranslationKey.WISHCOMP_TIMED_SATISFIED.Translate() : TranslationKey.WISHCOMP_TIMED_NOT_SATISFIED.Translate();
            desc += " (" + (Mathf.FloorToInt(holdingTime * 100f / GenDate.TicksPerDay) / 100f).ToString() + "/" + Props.daysToHold.ToString() + ")";
            return desc;
        }

    }
}
