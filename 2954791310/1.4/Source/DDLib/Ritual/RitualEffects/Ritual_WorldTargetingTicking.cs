﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DD
{
    public abstract class Ritual_WorldTargetingTicking : Ritual_WorldTargeting, ITickingRitual
    {
        private TimeKeeper duration = new TimeKeeper();
        private bool active = false;

        public int Duration => Def.GetModExtension<RitualTickingModExtension>().GetDuration(ActivationCount);
        public int DurationRemaining => duration.Remaining;
        public float DurationPercent => duration.RemainingPercent;

        public bool Active => active;
        public TickerType TickerType => Def.GetModExtension<RitualTickingModExtension>()?.tickerType ?? TickerType.Normal;

        public override bool CanActivate(float level) => duration.Expired && !Active && base.CanActivate(level);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref duration, "duration");
            Scribe_Values.Look(ref active, "active", false);
            if (duration == null)
            {
                duration = new TimeKeeper();
            }
        }

        public override void DoActivation()
        {
            active = true;
            duration.Update(Duration);
        }

        public override void DoDeactivation()
        {
            active = false;
        }

        public virtual void DoTick()
        {
            if(WorldTarget == null || WorldTarget.Destroyed || !WorldTarget.Spawned || duration.Expired)
            {
                if (Active)
                {
                    Deactivate();
                }
                return;
            }

            ApplyRitual(WorldTarget);
        }

        public void Tick()
        {
            if (TickerType == TickerType.Normal)
            {
                DoTick();
            }
        }

        public void TickRare()
        {
            if (TickerType == TickerType.Rare)
            {
                DoTick();
            }
        }

        public void TickLong()
        {
            if (TickerType == TickerType.Long)
            {
                DoTick();
            }
        }

        public override void Reset(bool resetActivationCount = false)
        {
            base.Reset(resetActivationCount);
            duration.Clear();
        }
    }
}
