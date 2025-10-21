using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetMostDislikedNonPartnerBedOwner))]
static class Patch_GetMostDislikedNonPartnerBedOwner
{
    // remove 'sharing bed' debuff
    static bool Prefix(Pawn p, ref Pawn __result){
        Building_Bed ownedBed = p.ownership.OwnedBed;
        if( ownedBed != null && ownedBed is Building_Cabin ){
            __result = null;
            return false;
        }

        return true;
    }
}
