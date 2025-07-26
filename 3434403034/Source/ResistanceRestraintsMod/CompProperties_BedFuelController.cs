using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ResistanceRestraintsMod
{
    public class CompProperties_BedFuelController : CompProperties
    {
        public CompProperties_BedFuelController()
        {
            this.compClass = typeof(CompBedFuelController);
        }
    }

    public class CompBedFuelController : ThingComp
    {
        private Building_Bed bed;
        private CompRefuelable refuelableComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            bed = parent as Building_Bed;
            refuelableComp = parent.GetComp<CompRefuelable>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (bed == null || refuelableComp == null) return;

            bool pawnInBed = false;
            foreach (var occupant in bed.CurOccupants)
            {
                if (occupant != null && occupant.Spawned)
                {
                    pawnInBed = true;
                    break;
                }
            }

            if (!pawnInBed)
            {
                if (!refuelableComp.Props.consumeFuelOnlyWhenUsed)
                {
                    refuelableComp.Props.consumeFuelOnlyWhenUsed = true;
                }
            }
            else
            {
                if (refuelableComp.Props.consumeFuelOnlyWhenUsed)
                {
                    refuelableComp.Props.consumeFuelOnlyWhenUsed = false;
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            return bed != null && bed.CurOccupants.Any() ? "Fuel Consumption: Active" : "Fuel Consumption: Inactive";
        }
    }
}