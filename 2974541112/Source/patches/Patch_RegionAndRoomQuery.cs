using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(RegionAndRoomQuery), nameof(RegionAndRoomQuery.GetDistrict))]
static class Patch_GetDistrict
{
    // fix base NullReferenceException in Building_Bed.DeSpawn() when CPS is a room for itself (outside)
    public static void Postfix(this Thing thing, ref District __result)
    {
        if( thing is Building_Base b && b.IsDespawning ){
            __result = null;
        }
    }
}
