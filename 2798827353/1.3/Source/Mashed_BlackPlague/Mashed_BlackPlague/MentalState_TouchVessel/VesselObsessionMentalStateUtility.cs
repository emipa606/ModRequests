using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mashed_BlackPlague
{
    public static class VesselObsessionMentalStateUtility
    {
		public static Thing GetClosestVessel(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
			Thing vessel = (Thing)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.BlackPlague_Vessel), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), 9999f, delegate (Thing x)
			{
				Thing vessel2 = x;
				return vessel2.TryGetComp<CompUseEffect_TouchVessel>().cooldownTicks == 0;
			}, null, 0, -1, false, RegionType.Set_Passable, false);
			if (vessel == null)
			{
				return null;
			}
			return vessel;
		}

		public static bool IsVesselValid(Thing vessel, Pawn pawn, bool ignoreReachability = false)
		{
			return vessel != null && vessel.Spawned && pawn.CanReserve(vessel, 1, -1, null, false) && vessel.TryGetComp<CompUseEffect_TouchVessel>().cooldownTicks == 0 &&(ignoreReachability || pawn.CanReach(vessel, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn));
		}
	}
}
