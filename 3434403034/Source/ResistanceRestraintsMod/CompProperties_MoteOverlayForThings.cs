using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace ResistanceRestraintsMod
{
    public class CompProperties_MoteOverlayForThings : CompProperties
    {
        public List<string> moteDefs; // List of motes to spawn
        public int tickMin = 60; // Minimum interval
        public int tickMax = 180; // Maximum interval

        public float sizeMin = 1f; // Minimum mote size
        public float sizeMax = 2f; // Maximum mote size

        public float velocityMin = 0.5f; // Minimum speed
        public float velocityMax = 1.5f; // Maximum speed

        public CompProperties_MoteOverlayForThings()
        {
            this.compClass = typeof(Comp_MoteOverlayForThings);
        }
    }

    public class Comp_MoteOverlayForThings : ThingComp
    {
        private int nextEffectTick = 0;

        private CompRefuelable compRefuelable => parent.GetComp<CompRefuelable>();
        private CompPowerTrader compPowerTrader => parent.GetComp<CompPowerTrader>();

        public CompProperties_MoteOverlayForThings Props => (CompProperties_MoteOverlayForThings)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ScheduleNextEffect();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Find.TickManager.TicksGame >= nextEffectTick)
            {
                SpawnRandomMote();
                ScheduleNextEffect();
            }
        }

        private void ScheduleNextEffect()
        {
            nextEffectTick = Find.TickManager.TicksGame + Rand.Range(Props.tickMin, Props.tickMax);
        }

        private void SpawnRandomMote()
        {
            if (parent == null || parent.Map == null || Props.moteDefs == null || Props.moteDefs.Count == 0)
                return;

            // Check if the building has power and/or fuel
            bool hasFuel = compRefuelable?.HasFuel ?? true; // If no refuelable component, assume true
            bool hasPower = compPowerTrader?.PowerOn ?? true; // If no power component, assume true

            if (!hasFuel || !hasPower) // If either runs out, stop spawning motes
                return;

            string selectedMote = Props.moteDefs.RandomElement();
            ThingDef moteDef = ThingDef.Named(selectedMote);

            if (moteDef == null)
            {
                return;
            }

            MoteThrown mote = (MoteThrown)ThingMaker.MakeThing(moteDef);
            if (mote == null)
                return;

            // Randomized properties
            float size = Rand.Range(Props.sizeMin, Props.sizeMax);
            float velocity = Rand.Range(Props.velocityMin, Props.velocityMax);
            float angle = Rand.Range(0, 360);

            mote.Scale = size;
            mote.exactPosition = parent.DrawPos;
            mote.SetVelocity(angle, velocity);

            GenSpawn.Spawn(mote, parent.Position, parent.Map);
        }
    }

}
