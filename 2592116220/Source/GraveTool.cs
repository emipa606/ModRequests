using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace BuriedAlive
{
    public static class GraveTool
    {
		public static Building_Grave FindGraveFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
		{
			IntVec3 position = p.Position;
			Map map = p.Map;
			ThingRequest thingReq = ThingRequest.ForDef(ThingDefOf.Grave);
			PathEndMode peMode = PathEndMode.ClosestTouch;
			TraverseParms traverseParams = TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false, false, false);
			float maxDistance = 9999f;
			Predicate<Thing> validator = (Thing x) =>
			{
				Debug.Log("validator: " + ((Building_Grave)x).AssignedPawn);
				return !((Building_Grave)x).HasAnyContents &&
				traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations) &&
				(((Building_Grave)x).AssignedPawn == null || ((Building_Grave)x).AssignedPawn == p);
			};
			Building_Grave grave = (Building_Grave)GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, maxDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
			if (grave != null)
			{
				return grave;
			}
			return null;
		}
	}
}
