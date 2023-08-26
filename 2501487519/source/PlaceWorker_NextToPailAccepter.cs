using RimWorld;
using System.Collections.Generic;
using Verse;


namespace AOBAStew
{
    public class PlaceWorker_NextToPailAccepter : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(
          BuildableDef checkingDef,
          IntVec3 loc,
          Rot4 rot,
          Map map,
          Thing thingToIgnore = null,
          Thing thing = null)
        {
            for (int index1 = 0; index1 < 4; ++index1)
            {
                IntVec3 c = loc + GenAdj.CardinalDirections[index1];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int index2 = 0; index2 < thingList.Count; ++index2)
                    {
                        ThingDef thingDef = GenConstruct.BuiltDefOf(thingList[index2].def) as ThingDef;
                        if (thingDef != null && thingDef.building != null && thingDef.building.wantsHopperAdjacent)
                            return (AcceptanceReport)true;
                    }
                }
            }
            return (AcceptanceReport)"FT_NextToStewPot".Translate();
        }
    }
}
