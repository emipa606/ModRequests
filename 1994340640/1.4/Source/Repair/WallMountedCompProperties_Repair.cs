using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace WallStuff
{
    class WallMountedCompProperties_Repair : CompProperties
    {
        public ThingDef thingToSpawn;

        public bool requiresPower;

        public string saveKeysPrefix;

        public WallMountedCompProperties_Repair()
        {
            this.compClass = typeof(WallMountedCompRepair);
        }
    }
}
