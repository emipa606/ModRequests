using RimWorld;
using Verse;

namespace zed_0xff.VNPE;

[StaticConstructorOnStartup]
public class Init {
    static Init() {
        var connected_bed = VThingDefOf.VNPE_ConnectedBed;
        if( connected_bed == null )
            return;

        if( connected_bed.GetCompProperties<CompProperties_AffectedByFacilities>() is CompProperties_AffectedByFacilities connected_props ){
            if (ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded")) {
                // Unlink dripper from ConnectedBed
                var dripper = DefDatabase<ThingDef>.GetNamed("VNPE_NutrientPasteDripper");
                if( connected_props.linkableFacilities.Contains(dripper) ){
                    connected_props.linkableFacilities.Remove(dripper);
                }
            }

        }
    }
}

