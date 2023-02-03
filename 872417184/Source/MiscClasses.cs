using System.Collections.Generic;
using Verse;

namespace MoreTimeInfo
{
    public enum ClockAccuracy : int
    {
        Analog,
        Atomic
    }
    public class CompProperties_Clock : CompProperties
    {
        public ClockAccuracy clockAccuracy = ClockAccuracy.Analog;
        public int ClockAccuracyInt
        {
            get
            {
                return (int)clockAccuracy;
            }
        }
        public CompProperties_Clock()
        {
            compClass = typeof(CompClock);
        }
    }
    /// <summary>
    /// Adaptation of Skullywag's work on better coolers.
    /// </summary>
    public class PlaceWorker_HangWall : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            IntVec3 loc2 = loc;
            loc2 = loc + GenAdj.CardinalDirections[rot.AsInt];
            Building edifice = loc2.GetEdifice(map);
            Building spaceOccupied = loc.GetEdifice(map);
            GenDraw.DrawFieldEdges(new List<IntVec3> { loc2 });
            if (edifice == null)
            {
                return "PlacementOnWall".Translate();
            }
            if (edifice.def == null)
            {
                return "PlacementOnWall".Translate();
            }
            if ((edifice.def.graphicData.linkFlags & LinkFlags.Wall) == LinkFlags.None)
            {
                return "PlacementOnWall".Translate();
            }
            return AcceptanceReport.WasAccepted;
        }
    }
}
