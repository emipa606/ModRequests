using Verse;

namespace CCL.CF
{

    public class PlaceWorker_WallAttachment : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            IntVec3 c = center - rot.FacingCell;

            Building support = c.GetEdifice(map);
            if (support == null)
            {
                return (AcceptanceReport)("MessagePlacementAgainstSupport".Translate());
            }

            if (
                (support.def == null) ||
                (support.def.graphicData == null)
            )
            {
                return (AcceptanceReport)("MessagePlacementAgainstSupport".Translate());
            }

            return (support.def.graphicData.linkFlags & (LinkFlags.Rock | LinkFlags.Wall)) != 0
                ? AcceptanceReport.WasAccepted
                    : (AcceptanceReport)("MessagePlacementAgainstSupport".Translate());
        }

    }

}