using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.LoftBed {
    [HarmonyPatch(typeof(Building_Bed), "GetCurOccupant")]
    static class Patch_GetCurOccupant
    {
        static void Postfix(Building_Bed __instance, int slotIndex, ref Pawn __result)
        {
            if ( __result == null )
                return;

            IntVec3 sleepingSlotPos = __instance.GetSleepingSlotPos(slotIndex);
            List<Thing> things = __instance.Map.thingGrid.ThingsListAt(sleepingSlotPos);
            List<Building_Bed> other_beds = new List<Building_Bed>();
            List<Pawn> pawns = new List<Pawn>();
            for (int i = 0; i < things.Count; i++){
                if( things[i] is Building_Bed bed ){
                    if( bed != __instance ){
                        other_beds.Add(bed);
                    }
                } else if (things[i] is Pawn pawn && pawn.CurJob != null && pawn.GetPosture().InBed()) {
                    pawns.Add(pawn);
                }
            }

            // if we are only the bed at specified coords => keep orignal result
            if ( other_beds.Count == 0 || pawns.Count == 0 ){
                return;
            }

            // there are at least one other bed and at least one pawn

            List<Pawn> owners = __instance.OwnersForReading;
            foreach( Pawn pawn in pawns ){
                if (owners.Contains(pawn)){
                    // if pawn is assigned to this bed => return it
                    __result = pawn;
                    return;
                }
            }

            foreach( Pawn pawn in pawns ){
                foreach( Building_Bed other_bed in other_beds ){
                    if (other_bed.OwnersForReading.Contains(pawn)){
                        // if pawn is assigned to another bed => return null
                        __result = null;
                        return;
                    }
                }
            }

            // keep original result?
        }
    }
}
