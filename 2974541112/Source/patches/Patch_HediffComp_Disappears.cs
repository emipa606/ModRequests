using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// notify TSS.AI that pawn's genes finished regrowing
[HarmonyPatch(typeof(HediffComp_Disappears), nameof(HediffComp_Disappears.CompPostPostRemoved))]
public static class Patch__HediffComp_Disappears__CompPostPostRemoved {
    static void Prefix(ref HediffComp_Disappears __instance){
        if(  __instance.Def != HediffDefOf.XenogermReplicating )
            return;

        if( __instance.Pawn.Spawned || !(__instance.Pawn.ParentHolder is Building_TSS tss))
            return;

        tss.ai.NotifyGenesFinishedRegrowing(__instance.Pawn);
    }
}
