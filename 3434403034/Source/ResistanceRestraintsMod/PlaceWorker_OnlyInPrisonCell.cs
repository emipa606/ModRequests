using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class PlaceWorker_OnlyInPrisonCell : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            Room room = loc.GetRoom(map);
            if (room == null || (room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.PrisonBarracks))
            {
                return new AcceptanceReport("Must be placed in a prison cell or prison barracks.");
            }
            return true;
        }
    }


}
