using Verse;

namespace ForceJobMod
{
    public class PlaceWorker_RequiresRoof : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            // Check if the location is roofed
            if (!map.roofGrid.Roofed(loc))
            {
                // Return a message if the placement is invalid
                return new AcceptanceReport("MustPlaceUnderRoof".Translate());
            }
            // Allow placement if the location is roofed
            return true;
        }
    }
}
