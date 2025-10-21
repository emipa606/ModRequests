using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// TSS: make them feel the beauty of TSS (or lack of it :)
[HarmonyPatch(typeof(Need_Beauty), "get_CurInstantLevel")]
static class Patch__Need_Beauty__get_CurInstantLevel
{
    static void Postfix(ref Need_Beauty __instance, ref float __result, Pawn ___pawn){
        if( ___pawn.Spawned ) return;
        if( PawnUtility.IsBiologicallyOrArtificiallyBlind(___pawn) ) return; // will be 0.5f

        if( ___pawn.ParentHolder is Building_TSS tss ){
            __result = __instance.def.baseLevel + tss.beauty * 0.1f;
        }
    }
}
