using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FactionBaseGeneration
{
    public class MapComponentGeneration : MapComponent
    {
        public MapComponentGeneration(Map map) : base(map)
        {

        }
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (this.DoGeneration && path.Length > 0)
            {
                SettlementGeneration.DoSettlementGeneration(this.map, this.path, this.locationDef, this.map.ParentFaction, false);
                this.DoGeneration = false;
            }
            //else if (path.Length > 0 && map.ParentFaction?.def == VoidDefOf.RH_VOID)
            //{
            //    foreach (var lord in map.lordManager.lords)
            //    {
            //        if (lord.LordJob is LordJob_DefendBase defendBase && lord.faction.HostileTo(Faction.OfPlayer) && lord.Graph.lordToils.IndexOf(lord.CurLordToil) >= 2)
            //        {
            //            var gameComp = Current.Game.GetComponent<VoidGameComp>();
            //            if (!gameComp.voidReinforcementsToSend.ContainsKey(map))
            //            {
            //                gameComp.voidReinforcementsToSend[map] = Find.TickManager.TicksAbs + new IntRange(1 * GenDate.TicksPerHour, 3 * GenDate.TicksPerHour).RandomInRange;
            //            }
            //        }
            //    }
            //}
            if (this.ReFog)
            {
                Log.Message("Refog" + this.map);
                FloodFillerFog.DebugRefogMap(this.map);
                this.ReFog = false;
            }
        }

        //public override void MapComponentTick()
        //{
        //    base.MapComponentTick();
        //    foreach (var locationDef in DefDatabase<LocationDef>.AllDefs)
        //    {
        //        Log.Message(Path.GetFullPath(locationDef.modContentPack.RootDir + "//" + locationDef.filePreset));
        //    }
        //}

        public void DoForcedGeneration(bool disableFog)
        {
            SettlementGeneration.DoSettlementGeneration(this.map, this.path, this.locationDef, this.map.ParentFaction, disableFog);
            this.DoGeneration = false;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.DoGeneration, "DoGeneration", false);
            Scribe_Values.Look<string>(ref this.path, "path", "");
        }

        public bool DoGeneration = false;

        public bool ReFog = false;
        public string path = "";
        public LocationDef locationDef;

    }
}

