using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class PlaceWorker_PreventAdjacentOverlap : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
            BuildableDef checkingDef,
            IntVec3 loc,
            Rot4 rot,
            Map map,
            Thing thingToIgnore = null,
            Thing thingToPlace = null)
        {
            // Define the surrounding 8 tiles to check
            foreach (IntVec3 adj in GenAdj.CellsAdjacentCardinal(loc, rot, checkingDef.Size))
            {
                // Ensure the tile is within the map bounds
                if (!adj.InBounds(map))
                    continue;

                // Check if there are any things in the adjacent cell
                foreach (Thing thing in map.thingGrid.ThingsListAtFast(adj))
                {
                    if (thing != thingToIgnore && thing.def.passability != Traversability.Standable)
                    {
                        return new AcceptanceReport("Must be placed with standable space on all sides.");
                    }
                }
            }
            return AcceptanceReport.WasAccepted;
        }
    }
}
