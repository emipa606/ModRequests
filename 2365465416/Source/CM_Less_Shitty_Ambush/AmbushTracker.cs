using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Less_Shitty_Ambush
{
    public class AmbushTracker : WorldComponent
    {
        private List<AmbushInfo> ambushInfos = new List<AmbushInfo>();

        public AmbushTracker(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
                ambushInfos = ambushInfos.Where(info => info.map != null && info.map.Parent != null && !info.map.Parent.Destroyed).ToList();

            Scribe_Collections.Look(ref ambushInfos, "ambushInfos", LookMode.Deep);
        }

        public int CheckMapCreateTime(Map map)
        {
            AmbushInfo ambushInfo = ambushInfos.Find(info => info.map == map);
            if (ambushInfo == null)
            {
                ambushInfo = new AmbushInfo(map, Find.TickManager.TicksGame);
                ambushInfos.Add(ambushInfo);
            }

            return ambushInfo.tickCreated;
        }
    }
}
