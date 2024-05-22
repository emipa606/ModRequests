using RimWorld;
using Verse;

namespace ReadingBill
{
    public class PlaceWorker_ReadingSpot : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (loc.Impassable(map) || !loc.HasEatSurface(map))
            {
                return "MustOnTable".Translate();
            }
            IntVec3 ic = ThingUtility.InteractionCellWhenAt(checkingDef as ThingDef, loc, rot, map);
            Building seat = ic.GetEdifice(map);
            if (seat == null
            || !seat.def.building.isSittable
            || (seat.def.rotatable && seat.Rotation != rot))
            {
                return "MustHaveSeat".Translate();
            }
            return true;
        }
    }
}
