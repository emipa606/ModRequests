using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.LoftBed {
    /*
     * does not work as expected :(
     *
    [HarmonyPatch(typeof(MoteMaker), nameof(MoteMaker.MakeAttachedOverlay))]
    static class Patch_MakeAttachedOverlay
    {
        // shift deathresting pawn flashing red mote when laying on a Loft Bed
        static void Postfix(ref Mote __result, Thing thing, ThingDef moteDef, Vector3 offset, float scale, float solidTimeOverride)
        {
            if( thing is Pawn pawn && moteDef == ThingDefOf.Mote_Deathresting ){
                Building_Bed bed = pawn.CurrentBed();
                if( CompLoftBed.isLoftBed(bed) ){
                    __result.yOffset += 0.5f;
                    __result.exactPosition.x += 20;
                    __result.exactPosition.z += 20;
                }
            }
        }
    }
    */
}
