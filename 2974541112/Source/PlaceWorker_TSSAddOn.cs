using System.Linq;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

public class PlaceWorker_TSSAddOn: PlaceWorker {
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null) {

        Building edifice = loc.GetEdifice( map );
        if( !(edifice is Building_TSS tss) ){
            return "TSS.MustBePlacedOn".Translate();
        }

        if( loc != tss.Position ){
            return "TSS.MustBePlacedOn".Translate();
        }

        if( checkingDef is ThingDef td && !td.comps.Any((CompProperties cp) => cp is CompProperties_TimeSpeed) ){
            // future non-time attachments
            return true;
        }

        if( tss.AffectedByFacilitiesComp.LinkedFacilitiesListForReading.Any((Thing t) => t.def.GetCompProperties<CompProperties_TimeSpeed>() != null) ) {
            return "TSS.OnlyOneTimeAddOnAllowed".Translate();
        }

        foreach (Thing thingHere in map.thingGrid.ThingsListAtFast(loc)){
            if( thingHere is Blueprint ){
                return "TSS.CannotPlaceOnBlueprint".Translate();
            }
        }

        return true;
    }
}
