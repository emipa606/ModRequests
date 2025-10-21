using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(Building_Bed), "get_SleepingSlotsCount")]
static class Patch_SleepingSlotsCount
{
    static bool Prefix(Building_Bed __instance, ref int __result)
    {
        if( __instance is Building_Base b ){
            __result = b.MaxSlots;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetSleepingSlotPos))]
static class Patch_GetSleepingSlotPos
{
    static bool Prefix(Building_Bed __instance, ref IntVec3 __result, int index){
        if( __instance is Building_Base ){
            int t = __instance.def.size.x * __instance.def.size.z;
            while( index >= t ){
                // over-assign extra slots, we'll fix draw position later
                index -= t;
            }
            int pos = 0;
            foreach (IntVec3 cell in __instance.OccupiedRect()){
                if( pos == index ){
                    __result = cell;
                    return false;
                }
                pos++;
            }
        }

        return true;
    }
}

// fix pawn in 5th slot unable to lay down
[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetCurOccupant))]
static class Patch_GetCurOccupant
{
    static bool Prefix(Building_Bed __instance, int slotIndex, ref Pawn __result)
    {
        if ( __instance is Building_Base b ){
            __result = b.GetCurOccupant(slotIndex);
            return false;
        }
        return true;
    }
}

// fix pawn in 5th slot unable to lay down
[HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetCurOccupantAt))]
static class Patch_GetCurOccupantAt
{
    static bool Prefix(Building_Bed __instance, IntVec3 pos, ref Pawn __result)
    {
        if ( __instance is Building_Base b ){
            __result = b.GetCurOccupantAt(pos);
            return false;
        }
        return true;
    }
}
