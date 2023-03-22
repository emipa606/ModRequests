using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
	public class WorkGiver_EmptyNutrientTube : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(CoreDefOf.NutrientTube);
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsOfDef(CoreDefOf.NutrientTube);
			for (int i = 0; i < list.Count; i++)
			{
				if (((Building_NutrientTube)list[i]).Produced)
				{
					return false;
				}
			}
			return true;
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_NutrientTube building_NutrientTube = t as Building_NutrientTube;
			return building_NutrientTube != null && building_NutrientTube.Produced && !t.IsBurning() && !t.IsForbidden(pawn) && pawn.CanReserve(t, 1, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(CoreDefOf.EmptyNutrientTube, t);
		}
	}
}
