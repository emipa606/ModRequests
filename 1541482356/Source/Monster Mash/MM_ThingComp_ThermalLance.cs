using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MonsterMash
{
    public class MM_ThingComp_ThermalLance : ThingComp
    {
        public MM_ThingComp_ThermalLance()
        {
            this.pawn = (Pawn)this.parent;
        }

        public Pawn Pawn
        {
            get
            {
                return this.pawn ?? (this.pawn = (Pawn)this.parent);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                Pawn.health.AddHediff(HediffDef.Named("MM_ThermalLance_ThermalCore"), pawn.RaceProps.body.AllParts.FirstOrDefault(part => part.Label == "heart"));
                Pawn.health.AddHediff(HediffDef.Named("MM_ThermalLance_OpticalTargetingSystem"), pawn.RaceProps.body.AllParts.FirstOrDefault(part => part.Label == "left eye"));
                Pawn.health.AddHediff(HediffDef.Named("MM_ThermalLance_FiringAssembly"), pawn.RaceProps.body.AllParts.FirstOrDefault(part => part.Label == "jaw"));
            }
        }

        private Pawn pawn;
    }
}
