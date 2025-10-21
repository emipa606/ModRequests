using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// debug Cabin
[HarmonyPatch(typeof(ReservationManager), nameof(ReservationManager.Release))]
public static class Patch__ReservationManager__Release
{
    public static void Prefix(LocalTargetInfo target, Pawn claimant, Job job) {
        if( Prefs.DevMode && target.Thing is Building_Cabin ){
            Log.Warning("[d] Release " + target.Thing + " by " + claimant);
        }
    }
}
