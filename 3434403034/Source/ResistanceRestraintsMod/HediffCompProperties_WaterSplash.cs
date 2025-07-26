using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_WaterEffect : HediffCompProperties
    {
        public List<string> moteDefs; // List of motes to spawn
        public int tickMin = 60; // Minimum interval
        public int tickMax = 180; // Maximum interval

        public float sizeMin = 1f; // Minimum mote size
        public float sizeMax = 2f; // Maximum mote size

        public float velocityMin = 0.5f; // Minimum speed
        public float velocityMax = 1.5f; // Maximum speed

        public HediffCompProperties_WaterEffect()
        {
            this.compClass = typeof(HediffComp_WaterEffect);
        }
    }

    public class HediffComp_WaterEffect : HediffComp
    {
        private int nextEffectTick = 0;

        public HediffCompProperties_WaterEffect Props => (HediffCompProperties_WaterEffect)this.props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ScheduleNextEffect();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

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
            if (Pawn == null || Pawn.Map == null || Props.moteDefs == null || Props.moteDefs.Count == 0)
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
            mote.exactPosition = Pawn.DrawPos;
            mote.SetVelocity(angle, velocity);

            GenSpawn.Spawn(mote, Pawn.Position, Pawn.Map);
        }
    }
}

