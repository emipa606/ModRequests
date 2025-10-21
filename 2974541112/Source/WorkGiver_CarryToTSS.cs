using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

public class WorkGiver_CarryToTSS : WorkGiver_CarryToMultiBuilding {
    public override ThingRequest ThingRequest => ThingRequest.ForDef(VDefOf.CPS_TSS);
}
