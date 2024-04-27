using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GardenCloche
{
    public class Cloche : Building
    {
        public ThingDef SelectedCrop;
        public int TicksToSpawn;
        public int HarvestCount;
        public int SpawnDelay;
        public int Power = 100;

        public int BaseHarvestCount;
        public int BaseSpawnDelay;

        public Cloche()
        {
            Log.Message("Cloche Init");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref this.SelectedCrop, "SelectedCrop");
            Scribe_Values.Look<int>(ref this.TicksToSpawn, "TicksToSpawn");
            Scribe_Values.Look<int>(ref this.HarvestCount, "HarvestCount");
            Scribe_Values.Look<int>(ref this.SpawnDelay, "SpawnDelay");
            Scribe_Values.Look<int>(ref this.BaseHarvestCount, "BaseHarvestCount");
            Scribe_Values.Look<int>(ref this.BaseSpawnDelay, "BaseSpawnDelay");
            Scribe_Values.Look<int>(ref this.Power, "Power");
            SetPowerConsumption(Power);
        }

        public void SetPowerConsumption(int watt)
        {
            GetComp<CompPowerTrader>().PowerOutput = watt * -1f;
        }
    }
}