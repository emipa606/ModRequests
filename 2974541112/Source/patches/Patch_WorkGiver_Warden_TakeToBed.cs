using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// Cabin: stop colonists constantly taking prisoners from the Cabin's district to the Cabin
[HarmonyPatch(typeof(WorkGiver_Warden_TakeToBed), nameof(WorkGiver_Warden_TakeToBed.TryMakeJob))]
static class Patch__WorkGiver_Warden_TakeToBed__TryMakeJob {
    static void Postfix(ref Job __result, Pawn pawn, Thing t){
        if( __result == null ) return;
        if( __result.def != JobDefOf.EscortPrisonerToBed ) return;

        Pawn prisoner = __result.targetA.Pawn;
        Building_Bed bed = __result.targetB.Thing as Building_Bed;

        if( prisoner == null || bed == null ) return;

        if( bed is Building_Cabin && prisoner.GetDistrict().Neighbors.Contains(bed.GetDistrict()) ){
            // cancel the job
            __result = null;
        }
    }
}
