using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld;

namespace Metro
{
    public class Filth_Web : Filth
    {
        private HiveAIManager hiveManager;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.hiveManager = map.GetComponent<HiveAIManager>();
        }
        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 30 == 0 && this.Map != null)
            {
                foreach (var t in this.Position.GetThingList(this.Map))
                {
                    if (t is Pawn pawn && !pawn.IsSpider())
                    {
                        if (pawn.health.hediffSet.GetFirstHediffOfDef(MetroDefOf.Metro_PathSlowdown) == null)
                        {
                            var hediff = HediffMaker.MakeHediff(MetroDefOf.Metro_PathSlowdown, pawn);
                            pawn.health.AddHediff(hediff);
                            if (hiveManager.spidersHiveCells.Contains(this.Position))
                            {
                                hiveManager.OrderSpidersInHiveToAttack(pawn);
                            }
                        }
                    }
                }
            }
        }
    }
}

