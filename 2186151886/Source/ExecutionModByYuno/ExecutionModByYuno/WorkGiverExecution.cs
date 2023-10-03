using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ExecutionModByYuno
{

	public class WorkGiverExecution : WorkGiver_Warden
	{

		

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.Building_ExecutionSpot);
			}

		}


		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.Building_ExecutionSpot).Cast<Thing>();
		}


		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{

		

			if (t is Building_SpotExecution)
			{
				Building_SpotExecution building = t as Building_SpotExecution;


				return pawn.CanReserve(t) && pawn.CanReserve(building.GetPrisonerToExecute(pawn));
			}
			else
				return false;

		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_SpotExecution building = t as Building_SpotExecution;
	
			return new Job(JobDefOf.JobDriver_HaulAndStartExecution, building.GetPrisonerToExecute(pawn), building, building.Position);

		}

	}
	}


