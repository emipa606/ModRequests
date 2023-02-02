using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Less_Shitty_Ambush
{
    public class AmbushInfo : IExposable
    {
        public Map map;
        public int tickCreated;
        public bool gotIt = true;

        public AmbushInfo()
        {

        }

        public AmbushInfo(Map ambushMap, int mapTickCreated)
        {
            map = ambushMap;
            tickCreated = mapTickCreated;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref tickCreated, "tickCreated");
        }
    }
}
