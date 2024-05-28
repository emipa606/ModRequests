using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.LoftBed {
    [HarmonyPatch(typeof(PawnUtility), "GainComfortFromCellIfPossible")]
    static class Patch_GainComfortFromCellIfPossible
    {
        // vanilla GainComfortFromCellIfPossible() might skip LoftBed if there's another building under it
        static bool Prefix(ref Pawn p, bool chairsOnly)
        {
            if (!p.Spawned || Find.TickManager.TicksGame % 10 != 0 || chairsOnly)
                return true;

            // returns only Loft beds!
            var loft_bed = BedCache.currentBedFor(p);
            if( loft_bed == null )
                return true;

            PawnUtility.GainComfortFromThingIfPossible(p, loft_bed);
            return false;
        }
    }
}
