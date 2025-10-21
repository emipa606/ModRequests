using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(RegionAndRoomUpdater), "ShouldBeInTheSameRoom")]
static class Patch_ShouldBeInTheSameRoom
{
    // mark CPS as a separate room, even outside w/o walls
    public static void Postfix(ref bool __result, District a, District b)
    {
        if( !__result ) return;

        Building_Base ba = Cache.Get(a.Cells.First(), a.Map);
        Building_Base bb = Cache.Get(b.Cells.First(), b.Map);
        if( ba != null || bb != null ){
            __result = false;
        }
    }
}
