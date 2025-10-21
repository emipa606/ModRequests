using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// TSS: make them feel in cramped space, even outside
// prefix should be a bit faster than vanilla
[HarmonyPatch(typeof(ThoughtWorker_NeedRoomSize), "CurrentStateInternal")]
static class Patch__ThoughtWorker_NeedRoomSize__CurrentStateInternal
{
    static bool Prefix(ref ThoughtState __result, Pawn p){
        if( p.Spawned ) return true;
        if( !(p.ParentHolder is Building_TSS) ) return true;

        __result = ThoughtState.ActiveAtStage(0); // very cramped
        return false;
    }
}
