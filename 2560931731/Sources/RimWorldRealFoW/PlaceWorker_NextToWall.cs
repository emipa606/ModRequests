using System;
using Verse;
using RimWorld;

namespace RimWorldRealFoW
{
	public class PlaceWorker_NextToWall : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			IntVec3 c = loc - rot.FacingCell;
			Building edifice = c.GetEdifice(map);
			if (!c.InBounds(map) || !loc.InBounds(map))
			{
				return  false;
			}
			else
			{
				//Additional joy object's code
				if (
					edifice == null 
					|| edifice.def == null 
					|| (edifice.def != ThingDefOf.Wall 
					&& !edifice.def.IsSmoothed 
					&& (edifice.Faction == null 
						|| edifice.Faction != Faction.OfPlayer 
						|| edifice.def.graphicData == null 
						|| edifice.def.graphicData.linkFlags == LinkFlags.None 
						|| (LinkFlags.Wall & edifice.def.graphicData.linkFlags) == LinkFlags.None)))
				{
					 return new AcceptanceReport("MustBeNextToWall".Translate());

				}
				else
				{
					return true;
				}
			}

		}

	}
}
