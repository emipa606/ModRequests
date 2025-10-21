using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// TSS: make them feel a little comfort
// should be faster than iterating pawns each 10 ticks in TSS' Tick()
[HarmonyPatch(typeof(Need_Comfort), "get_CurInstantLevel")]
static class Patch__Need_Comfort__CurInstantLevel
{
    static void Postfix(ref float __result, Pawn ___pawn){
        if( ___pawn.Spawned ) return;

        if( ___pawn.ParentHolder is Building_TSS tss ){
            __result = tss.comfort;
        }
    }
}
