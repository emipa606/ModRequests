using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// debug Cabin
[HarmonyPatch(typeof(ReservationUtility), nameof(ReservationUtility.CanReserve))]
public static class Patch_CanReserve
{
    public static void Postfix(ref bool __result, Pawn p, LocalTargetInfo target){
        if( Prefs.DevMode && target.Thing is Building_Cabin ){
            Log.Warning("[d] CanReserve " + target.Thing + " by " + p + " => " + __result);
        }
    }
}
