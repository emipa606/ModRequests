using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.LoftBed {
    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    [HarmonyPriority(Priority.Last)] // make it last to fix any offsets by other mods, fixes Yayo's animations
    static class Patch_GetBodyPos
    {
        private static readonly AccessTools.FieldRef<PawnRenderer, Pawn> _pawn = AccessTools.FieldRefAccess<PawnRenderer, Pawn>("pawn");

        // shift pawn's head position according to LoftBed's shift
        static void Postfix(PawnRenderer __instance, Vector3 drawLoc, ref bool showBody, ref Vector3 __result)
        {
            Pawn pawn = _pawn(__instance);
            if( pawn == null ) return;

            int? sleepingSlot;
            Building_Bed bed = pawn.CurrentBed(out sleepingSlot);
            if ( BedCache.isLoftBed(bed) ){
                // can also fine-tune pawn's head altitude here, but Patch_MoteMaker should also be changed then
                if( LoftBedMod.Settings.altPerspectiveMode ){
                    __result.z += LoftBedMod.Settings.f2;
                } else {
                    if( bed.Rotation == Rot4.South ){
                        __result.z = bed.Position.z + LoftBedMod.Settings.f2;
                    } else if( bed.Rotation == Rot4.North ){
                        __result.z = bed.Position.z + LoftBedMod.Settings.f2 + 1.0f;
                    } else if( bed.Rotation == Rot4.East ){
                        if( sleepingSlot == null ) sleepingSlot = 0;
                        __result.z = bed.Position.z + LoftBedMod.Settings.f2 + 0.5f - (int)sleepingSlot;
                    } else {
                        if( sleepingSlot == null ) sleepingSlot = 0;
                        __result.z = bed.Position.z + LoftBedMod.Settings.f2 + 0.5f + (int)sleepingSlot;
                    }
                }
            }
        }
    }
}
