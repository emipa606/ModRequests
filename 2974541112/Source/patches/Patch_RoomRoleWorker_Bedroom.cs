using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// set any room with a cabin to be barracks
[HarmonyPatch(typeof(RoomRoleWorker_Bedroom), "IsBedroomHelper")]
public static class Patch_IsBedroomHelper
{
    public static bool Prefix(List<Building_Bed> beds, ref bool __result)
    {
        if (beds.Any((Building_Bed x) => x is Building_Cabin))
        {
            __result = false;
            return false;
        }
        return true;
    }
}
