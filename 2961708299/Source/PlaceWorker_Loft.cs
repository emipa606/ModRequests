using Verse;
using System.Collections.Generic;
using RimWorld;

namespace zed_0xff.LoftBed
{
	public class PlaceWorker_Loft : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
            List<IntVec3> nearLocs = new List<IntVec3>();

            if( def.Size.x == 1 ){
                // single bed
                if( rot == Rot4.South ){
                    nearLocs.Add(new IntVec3(loc.x, loc.y, loc.z+1));
                    nearLocs.Add(new IntVec3(loc.x, loc.y, loc.z-def.Size.z));
                } else if( rot == Rot4.North ){
                    nearLocs.Add(new IntVec3(loc.x, loc.y, loc.z-1));
                    nearLocs.Add(new IntVec3(loc.x, loc.y, loc.z+def.Size.z));
                } else if( rot == Rot4.East ){
                    nearLocs.Add(new IntVec3(loc.x-1, loc.y, loc.z));
                    nearLocs.Add(new IntVec3(loc.x+def.Size.z, loc.y, loc.z));
                } else {
                    nearLocs.Add(new IntVec3(loc.x+1, loc.y, loc.z));
                    nearLocs.Add(new IntVec3(loc.x-def.Size.z, loc.y, loc.z));
                }
            } else if (def.Size.x == 2){
                // double bed
                CellRect newRect = GenAdj.OccupiedRect(loc, rot, def.Size);
                nearLocs.Add(new IntVec3(newRect.minX-1, loc.y, newRect.minZ-1));
                nearLocs.Add(new IntVec3(newRect.maxX+1, loc.y, newRect.maxZ+1));
                nearLocs.Add(new IntVec3(newRect.minX-1, loc.y, newRect.maxZ+1));
                nearLocs.Add(new IntVec3(newRect.maxX+1, loc.y, newRect.minZ-1));
            }

            List<Thing> nearThings = new List<Thing>();
            foreach (IntVec3 nearLoc in nearLocs){
                foreach (Thing thingNear in map.thingGrid.ThingsListAtFast(nearLoc)){
                    if (!(thingNear is Building || thingNear is Blueprint))
                        continue;
                    if (thingNear.def.Size.x == 1 || thingNear.def.Size.z == 1)
                        continue;
                    if (thingNear.def.altitudeLayer > def.altitudeLayer || !thingNear.def.selectable){
                        // Dubs Skylights
                        continue;
                    }
                    nearThings.Add(thingNear);
                }
            }

			foreach (IntVec3 cell in GenAdj.OccupiedRect(loc, rot, def.Size))
			{
				foreach (Thing thingHere in map.thingGrid.ThingsListAtFast(cell))
				{
                    if (thingHere == thingToIgnore )
                        continue;

					if (thingHere is Building building && ((isForbidden(building.def)) || BedCache.isLoftBed(building)))
                        return new AcceptanceReport("WorkPlacer_CannotPlaceHere".Translate());

					if (thingHere is Blueprint blueprint && isForbidden(blueprint.def.entityDefToBuild as ThingDef))
                        return new AcceptanceReport("WorkPlacer_CannotPlaceHere".Translate());

                    if (nearThings.Contains(thingHere)){
                        return new AcceptanceReport("WorkPlacer_CannotPlaceHere".Translate());
                    }
				}
			}
			return true;
		}

		static bool isForbidden(ThingDef def)
		{
			return (def != null && (def == VThingDefOf.LoftBed || def.fillPercent > LoftBedMod.Settings.maxFillPercent || def.holdsRoof));
		}
	}
}
