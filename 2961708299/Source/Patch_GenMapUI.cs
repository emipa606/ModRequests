using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.LoftBed {
    [HarmonyPatch(typeof(GenMapUI), nameof(GenMapUI.LabelDrawPosFor), new[] {typeof(Thing), typeof(float)})]
    static class Patch_LabelDrawPosFor
    {
        // shift pawn name label according to LoftBed's shift
        static void Postfix(ref Vector2 __result, Thing thing, float worldOffsetZ)
        {
            if (thing is Pawn pawn){
                Building_Bed bed = pawn.CurrentBed();
                if ( bed != null && bed.def == VThingDefOf.LoftBed ) {
                    __result.y += LoftBedMod.Settings.f1;
                }
            } else if ( BedCache.isLoftBed(thing) ){
                __result.y += LoftBedMod.Settings.f1;
            }
        }
    }
}
