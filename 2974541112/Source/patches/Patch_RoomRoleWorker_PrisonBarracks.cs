using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// set any room with a prison cabin to be prison barracks
[HarmonyPatch(typeof(RoomRoleWorker_PrisonBarracks), nameof(RoomRoleWorker_PrisonBarracks.GetScore))]
public static class Patch__RoomRoleWorker_PrisonBarracks__GetScore {
    public static void Postfix(ref float __result, Room room) {
        if( __result != 0f ) return;

        int nCabins = 0;
        List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
        for (int i = 0; i < containedAndAdjacentThings.Count; i++) {
            if (containedAndAdjacentThings[i] is Building_Cabin cabin) {
                if (!cabin.ForPrisoners) {
                    return;
                }
                nCabins++;
            }
        }

        if( nCabins > 0 ){
            __result = nCabins * 100100f * 10; // cabin has 10 sleeping slots
        }
    }
}
