using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class HediffWishInfo : IExposable
    {
        public HediffDef def;

        public int stageToReach = -1;

        public float severityToReach = -1;

        public bool countAllMatch = true;


        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref stageToReach, "stageToReach", -1);
            Scribe_Values.Look(ref severityToReach, "severityToReach", -1);
            Scribe_Values.Look(ref countAllMatch, "countAllMatch", true);
        }

    }
}
