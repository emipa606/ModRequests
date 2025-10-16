using Verse;
using RimWorld;

namespace ForceJobMod
{
    [DefOf]
    public static class StrippingTableDefOf
    {
        static StrippingTableDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StrippingTableDefOf));
        }

        public static ThingDef StrippingTableTable;
    }

    public class PlaceWorker_CameraNearStrippingTable : PlaceWorker
    {
        private const int MinDistance = 2; // Define the minimum allowed distance
        private const int MaxDistance = 5; // Define the maximum allowed distance

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (Thing thingInRadius in map.listerThings.ThingsOfDef(StrippingTableDefOf.StrippingTableTable))
            {
                float distance = loc.DistanceTo(thingInRadius.Position);
                if (distance >= MinDistance && distance <= MaxDistance)
                {
                    return true;
                }
            }

            return "MustPlaceCameraNearStrippingTable".Translate(MinDistance, MaxDistance);
        }
    }
}
