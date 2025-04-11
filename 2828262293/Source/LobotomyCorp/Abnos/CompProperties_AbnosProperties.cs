using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;


namespace LobotomyCorp.Abnos
{
    public class CompProperties_AbnosProperties : CompProperties
    {

        public Danger danger = Danger.ZAYIN;
        public Virtues virtues;

        public int pbox = 8;
        public int qcounter = 0;

        public bool isTool = false;

        public SoundDef sound;

        public struct Virtues
        {
            public float Fortitude;
            public float Prudence;
            public float Temperance;
            public float Justice;

            public string DebugText => "F:"+Fortitude +" P:"+Prudence + " T:"+ Temperance + " J:"+Justice;
        }

        public CompProperties_AbnosProperties() 
        {
            this.compClass = typeof(CompAbnos_Base);
        }
    }

    public class CompAbnos_Base : ThingComp
    {
        protected int count = 0;
        protected static readonly int TICK_TIME = 60;

        protected int jobTick = -1;
        protected static readonly int TIME_NOJOB = -1;
        protected static readonly int TIME_ACTJOB = 0;

        public Command action;

        public bool approached = false;
        public bool finished = false;

        public Pawn actor = null;

        public Thing core = null;

        public Pawn parentPawn => parent as Pawn;

        public CompAbnos_Base() { }

        public virtual IEnumerable<Gizmo> CoreGizmos()
        {
            yield return action;
        }

        public override void CompTick()
        {
            count = count > TICK_TIME ? 0 : count + 1;
            if (IsTick) return;

            
        }

        public virtual bool IsTick => count % TICK_TIME != TICK_TIME - 1;
    }

}
