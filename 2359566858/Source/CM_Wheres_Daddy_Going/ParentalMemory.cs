using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    public class ParentalMemory : IExposable
    {
        public Pawn pawn;
        public ThingDef thingDef;
        public bool gotIt = true;

        public ParentalMemory()
        {

        }

        public ParentalMemory(Pawn parent)
        {
            pawn = parent;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref gotIt, "gotIt");
        }
    }
}
