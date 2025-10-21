using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// Debug Cabin
[HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.TryFindFreeSittingSpotOnThing))]
static class Patch_TryFindFreeSittingSpotOnThing
{
    static void Postfix(Thing t, Pawn pawn, ref IntVec3 cell, bool __result){
        if( Prefs.DevMode && t is Building_Cabin ){
            if( __result ){
                Log.Warning("[d] TryFindFreeSittingSpotOnThing " + t + ": " + pawn + ": " + cell);
            } else {
                Log.Warning("[d] TryFindFreeSittingSpotOnThing " + t + ": " + pawn + ": " + __result);
            }
        }
    }
}
