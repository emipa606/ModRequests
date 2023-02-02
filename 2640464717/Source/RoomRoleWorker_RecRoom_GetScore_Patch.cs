using HarmonyLib;
using RimWorld;
using Verse;

namespace TorchesArentRecRoomsForBetterPyromania
{
    [HarmonyPatch(typeof(RoomRoleWorker_RecRoom), nameof(RoomRoleWorker_RecRoom.GetScore))]
    static class RoomRoleWorker_RecRoom_GetScore_Patch
    {
        public static float Postfix(float __result, Room room)
        {
            foreach (var thing in room.ContainedAndAdjacentThings)
            {
                if (thing.def.category == ThingCategory.Building && PyromaniaDefOf.Pyromaniac_WatchFlame.thingDefs.Contains(thing.def))
                {
                    __result -= 5f;
                }
            }

            return __result;
        }
    }
}
