using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public abstract class WorkGiver_TendBloodSugar : WorkGiver_FeedPatient
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsWithAnyHediff;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawn == pawns[i])
				{
					continue;
				}

				if (DiabetesUtility.HasIrregularBloodSugar(pawns[i]))
				{
					yield return pawns[i];
				}
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn) t;
			if (pawn2 == null || pawn2 == pawn)
			{
				return false;
			}
			if(!FeedPatientUtility.ShouldBeFed(pawn2))
			{
				return false;
			}
			//if (pawn2.RaceProps.Humanlike && !pawn2.InBed())
			//{
			//	return false;
			//}
			if (!pawn.CanReserve(pawn2, ignoreOtherReservations: forced))
			{
				return false;
			}
			return true;
		}

		public override abstract Job JobOnThing(Pawn pawn, Thing t, bool forced = false);
	}
}
