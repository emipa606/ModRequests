using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach), typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms))]
static class Patch_Reachability_CanReach
{
    // don't allow prisoners to get out of the pit themselves
    public static void Postfix(ref bool __result, IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParams)
    {
        if( __result == false ) return;
        if( traverseParams.pawn == null ) return;
        if( traverseParams.pawn.Map == null ) return;

        Map map = traverseParams.pawn.Map;
        Pawn pawn = traverseParams.pawn;
        if( !pawn.IsPrisonerOfColony ) return;

        District srcDistrict = RegionAndRoomQuery.DistirctAtFast(start, map);
        District dstDistrict = RegionAndRoomQuery.DistirctAtFast(dest.Cell, map);
        if( srcDistrict == dstDistrict ) return;

        Building_ThePit srcPit = Cache.Get(start, map) as Building_ThePit;
        if( srcPit == null ) return;

        Building_ThePit dstPit = Cache.Get(dest.Cell, map) as Building_ThePit;
        __result = (srcPit == dstPit);
    }
}
