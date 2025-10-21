using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(MentalBreaker), "get_CanDoRandomMentalBreaks")]
static class Patch_CanDoRandomMentalBreaks
{
    // do random mental breaks inside unpowered TSS
    public static void Postfix(ref bool __result, ref Pawn ___pawn){
        if( __result ) return;

        if( ___pawn.ParentHolder is Building_TSS tss && tss.IsInternalBatteryEmpty() ){
            __result = true;
        }
    }
}
