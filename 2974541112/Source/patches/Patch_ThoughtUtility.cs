using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// TSS: allow pawn to get feelings abouth room size and beauty while inside TSS
[HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.CanGetThought))]
static class Patch_CanGetThought
{
    static void Postfix(ref bool __result, Pawn pawn, ThoughtDef def){
        if( __result ) return;
        if( def != VDefOf.NeedRoomSize && def != VDefOf.NeedBeauty ) return;
        if( !(pawn.ParentHolder is Building_TSS) ) return;

        __result = true;
    }
}
