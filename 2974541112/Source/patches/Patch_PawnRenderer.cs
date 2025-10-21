using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.CPS;

// called for any Find.CameraDriver.ZoomRootSize
// XXX called only for sleeping pawns
[HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
[HarmonyPriority(Priority.Last)] // make it last to fix any offsets by other mods, fixes Yayo's animations
static class Patch_GetBodyPos
{
    static void Postfix(PawnRenderer __instance, Vector3 drawLoc, ref bool showBody, ref Vector3 __result, Pawn ___pawn)
    {
        if( ___pawn == null || !___pawn.RaceProps.Humanlike ) return;

        Building_Base b = Cache.Get(___pawn.Position, ___pawn.Map);
        if( b == null ) return;

        if( ___pawn.GetPosture().InBed()){
            // draw only heads of sleeping pawns
            showBody = false;
            b.FixSleepingPawnHeadPos(ref ___pawn, ref __result);
        }
    }
}


// XXX called only when Find.CameraDriver.ZoomRootSize < 18f
// called for both sleeping and not sleeping pawns
[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
[HarmonyPriority(Priority.Last)] // TODO: check with Yayo's animations
static class Patch_RenderPawnInternal
{
    static void Prefix(ref PawnRenderer __instance, ref Vector3 rootLoc, ref bool renderBody, Pawn ___pawn){
        if( !___pawn.RaceProps.Humanlike ) return;

        Building_Base b = Cache.Get(___pawn.Position, ___pawn.Map);
        if( b == null ) return;

        if( b is Building_ThePit && ___pawn.IsPrisonerOfColony && !___pawn.GetPosture().InBed() ){
            // hide bodies of not sleeping prisoners
            renderBody = false;
            rootLoc.z -= 0.4f;
        }
    }
}


// XXX called only when Find.CameraDriver.ZoomRootSize > 18f
//    [HarmonyPatch(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame")]
//    [HarmonyPriority(Priority.Last)] // not sure
//    static class Patch_GetBlitMeshUpdatedFrame
//    {
//        static void Prefix(ref PawnRenderer __instance, ref PawnDrawMode drawMode){
//            Pawn pawn = _pawn(__instance);
//            if( !pawn.RaceProps.Humanlike ) return;
//
//            Building_Base b = Cache.Get(pawn.Position, pawn.Map);
//            if( b == null ) return;
//
//            drawMode = PawnDrawMode.HeadOnly;
//        }
//    }

// fix pawn head rotation for any Find.CameraDriver.ZoomRootSize
// cached, requires pawn to go out of bed and back to update
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BodyAngle))]
[HarmonyPriority(Priority.Last)] // not sure
static class Patch_BodyAngle
{
    static float Postfix(float __result, ref PawnRenderer __instance, Pawn ___pawn){
        if( __result == 0f ) return __result; // most frequent case

        if( !___pawn.RaceProps.Humanlike ) return __result;

        Building_Bed bed = ___pawn.CurrentBed();
        if( bed is Building_Base ){
            return 0f;
        }
        return __result;
    }
}

// draw no shadow for not sleeping pawns in the pit
[HarmonyPatch(typeof(PawnRenderer), "DrawInvisibleShadow")]
static class Patch_DrawInvisibleShadow
{
    static bool Prefix(ref PawnRenderer __instance, ref Vector3 drawLoc, Pawn ___pawn){
        if( ___pawn.IsPrisonerOfColony && Cache.Get(___pawn.Position, ___pawn.Map) is Building_ThePit ){
            return false;
        }
        return true;
    }
}
