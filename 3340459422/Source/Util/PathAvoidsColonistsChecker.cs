// Modified by llunak, l.lunak@centrum.cz .

using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;

namespace SR.ModRimWorld.RaidExtension.Util
{
    // Checks if direct path from one tile to another avoids the colony. It checks for colonists, with the assumption
    // that they generally will be somewhere in the colony, so this should avoids most of the colony itself too.
    // This does not consider buildings the way Based on RCellFinder.TryFindEdgeCellWithPathToPositionAvoidingColony()
    // does (even though the code is based on it), because some buildings like geothermal generators or wells
    // (from Dub's Bad Hygiene) can be often placed out of the colony, and that way it could be hard to find a path.
    public class PathAvoidsColonistsChecker
    {
        private List<IntVec3> tmpSpotsToAvoid = new List<IntVec3>();
        private Map map;
        private IntVec3 pos;
        private readonly bool flag = false; // Debug.

        public static IntVec3 FindPathDestination( Map map, IntVec3 start )
        {
            PathAvoidsColonistsChecker pathChecker = new PathAvoidsColonistsChecker( map, start );
            int minDistToColonists = 100;
            Danger danger = Danger.None;
            for( int attempt = 0; attempt < 50; ++attempt )
            {
                if( attempt == 10 || attempt == 40 )
                    minDistToColonists = 50;
                else if( attempt == 25 )
                {
                    minDistToColonists = 100;
                    danger = Danger.Some;
                }
                if (!RCellFinder.TryFindTravelDestFrom( start, map, out var travelDest ))
                    continue;
                if( pathChecker.CheckPathAvoidsColonists( travelDest, minDistToColonists, danger ))
                    return travelDest;
            }
            return IntVec3.Invalid;
        }

        public PathAvoidsColonistsChecker(Map _map, IntVec3 _pos)
        {
            map = _map;
            pos = _pos;
            foreach (Pawn item2 in map.mapPawns.FreeColonistsAndPrisonersSpawned)
                tmpSpotsToAvoid.Add(item2.Position);
            if (flag)
            {
                for (int i = 0; i < tmpSpotsToAvoid.Count; i++)
                    map.debugDrawer.FlashCell(tmpSpotsToAvoid[i], 1f);
            }
        }

        public bool CheckPathAvoidsColonists( IntVec3 target, float minDistToColonists, Danger danger )
        {
            float num = minDistToColonists * minDistToColonists;
            if (target.Roofed(map) || !target.Standable(map) || !map.reachability.CanReach(pos, target, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, danger))
                return false;
            foreach (IntVec3 item3 in GenSight.PointsOnLineOfSight(pos, target))
            {
                for (int j = 0; j < tmpSpotsToAvoid.Count; j++)
                {
                    if ((tmpSpotsToAvoid[j] - item3).LengthHorizontalSquared > num)
                        continue;
                    if( flag )
                    {
                        map.debugDrawer.FlashLine(tmpSpotsToAvoid[j], item3, 50, SimpleColor.Blue);
                        map.debugDrawer.FlashLine(pos, target, 50, SimpleColor.Red);
                    }
                    return false;
                }
            }
            if( flag )
                map.debugDrawer.FlashLine(pos, target, 50, SimpleColor.Green);
            return true;
        }
    }
}
