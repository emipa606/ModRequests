using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class PlaceWorker_ByWall : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            List<IntVec3> cells = GenAdjFast.AdjacentCellsCardinal(loc, rot, ((ThingDef)checkingDef).size);
            foreach (IntVec3 cell in cells)
            {
                if (cell.GetEdifice(map)?.def.building.isPlaceOverableWall ?? false)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class PlaceWorker_ByWallOnSouth : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            LinkDirections directions = LinkDirections.None;
            switch (rot.AsInt)
            {
                case 0:
                    directions = LinkDirections.Down;
                    break;
                case 1:
                    directions = LinkDirections.Left;
                    break;
                case 2:
                    directions = LinkDirections.Up;
                    break;
                case 3:
                    directions = LinkDirections.Right;
                    break;
            }
            IEnumerable<IntVec3> cells = CellsAdjacentEdge(loc, rot, ((ThingDef)checkingDef).size, directions);
            Map currentMap = Find.CurrentMap;
            bool placeable = true;
            foreach (IntVec3 cell in cells)
            {
                IntVec3 c = cell;
                if (!c.InBounds(currentMap))
                {
                    continue;
                }
                if (c.GetEdifice(map)?.def.building.isPlaceOverableWall ?? false)
                {
                    if (c == ThingUtility.InteractionCellWhenAt((ThingDef)checkingDef, loc, rot, map))
                    {
                        GhostDrawer.DrawGhostThing(c, rot, ThingDefOf.Door, ThingDefOf.Door.graphic, Color.red, AltitudeLayer.MetaOverlays);
                        placeable = false;
                    }
                    else
                    {
                        GhostDrawer.DrawGhostThing(c, rot, ThingDefOf.Wall, ThingDefOf.Wall.graphic, Color.green, AltitudeLayer.MetaOverlays);
                    }
                }
                else
                {
                    if (c == ThingUtility.InteractionCellWhenAt((ThingDef)checkingDef, loc, rot, map))
                    {
                        GhostDrawer.DrawGhostThing(c, rot, ThingDefOf.Door, ThingDefOf.Door.graphic, Color.green, AltitudeLayer.MetaOverlays);
                    }
                    else
                    {
                        GhostDrawer.DrawGhostThing(c, rot, ThingDefOf.Wall, ThingDefOf.Wall.graphic, Color.red, AltitudeLayer.MetaOverlays);
                        placeable = false;
                    }
                }
            }
            return placeable;
        }

        public static IEnumerable<IntVec3> CellsAdjacentEdge(IntVec3 thingCent, Rot4 thingRot, IntVec2 thingSize, LinkDirections dir)
        {
            GenAdj.AdjustForRotation(ref thingCent, ref thingSize, thingRot);
            int minX = thingCent.x;
            int minZ = thingCent.z;
            int maxX = minX + thingSize.x - 1;
            int maxZ = minZ + thingSize.z - 1;
            if (dir == LinkDirections.Down)
            {
                for (int x4 = minX; x4 <= maxX; x4++)
                {
                    yield return new IntVec3(x4 - 1, thingCent.y, minZ - 1);
                }
            }
            if (dir == LinkDirections.Up)
            {
                for (int x4 = minX; x4 <= maxX; x4++)
                {
                    yield return new IntVec3(x4 - 1, thingCent.y, maxZ + 1);
                }
            }
            if (dir == LinkDirections.Left)
            {
                for (int x4 = minZ; x4 <= maxZ; x4++)
                {
                    yield return new IntVec3(minX - 1, thingCent.y, x4 - 1);
                }
            }
            if (dir == LinkDirections.Right)
            {
                for (int x4 = minZ; x4 <= maxZ; x4++)
                {
                    yield return new IntVec3(maxX + 1, thingCent.y, x4 - 1);
                }
            }
        }
    }

    public class PlaceWorker_InRoom : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            return loc.GetRoom(map) != null;
        }
    }
}
