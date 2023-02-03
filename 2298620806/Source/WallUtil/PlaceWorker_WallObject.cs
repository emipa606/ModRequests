using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WallHeater
{
    class PlaceWorker_WallObject : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            IntVec3 c = center;
            Building wall = c.GetEdifice(map);

            if (wall == null)
            {
                return ResourceBank.NeedsWall;
            }

            if ((wall.def == null) || (wall.def.graphicData == null))
            {
                return ResourceBank.NeedsWall;
            }

            return IsWallOrRock(wall) ? AcceptanceReport.WasAccepted : ResourceBank.NeedsWall;
        }

        private static bool IsWallOrRock(Building wall)
        {
            return ((wall.def.graphicData.linkFlags & LinkFlags.Wall) != 0) || ((wall.def.graphicData.linkFlags & LinkFlags.Rock) != 0);
        }
    }
}
