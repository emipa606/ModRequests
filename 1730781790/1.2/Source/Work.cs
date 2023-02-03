using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace HamsterWheel
{
    public class WorkGiver_HamsterWheel : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                //Log.Message("WorkGiver_HamsterWheel.PotentialWorkThingRequest");
                return ThingRequest.ForDef(ThingDefOf.HamsterWheelGenerator);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.HamsterWheelGenerator).Cast<Thing>();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            //Log.Message("WorkGiver_HamsterWheel.ShouldSkip");
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                if (allBuildingsColonist[i].def == ThingDefOf.HamsterWheelGenerator)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("WorkGiver_HamsterWheel.HasJobOnThing");
            Building building = t as Building;
            bool flag = building == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                if (building.IsForbidden(pawn))
                {
                    result = false;
                }
                else
                {
                    LocalTargetInfo target = building;
                    if (!pawn.CanReserve(target, 1, -1, null, forced))
                    {
                        //Log.Message("WorkGiver_HamsterWheel.pawn.CanReserve = false");
                        result = false;
                    }
                    else
                    {
                        //Log.Message("WorkGiver_HamsterWheel.pawn.CanReserve = true");
                        CompPowerPlantHamsterWheel compHW = building.TryGetComp<CompPowerPlantHamsterWheel>();
                        result = (compHW.CanUseNow &&  !building.Position.IsInPrisonCell(pawn.Map) && !building.IsBurning());
                    }
                }
            }
            return result;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("WorkGiver_HamsterWheel.JobOnThing");
            return new Job(JobDefOf.Job_HamsterWheel, t, 1500, true);
        }
    }
}
