using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

static class Patch_RestUtility {
    // without this (almost) any 1st cabin sleeper is not willing to share the cabin)
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.BedOwnerWillShare))]
    static class Patch_BedOwnerWillShare
    {
        static void Postfix(Building_Bed bed, ref bool __result)
        {
            if( __result || !(bed is Building_Cabin) ) return;

            __result = bed.AnyUnownedSleepingSlot;
        }
    }

    // a weird empty patch that fixes case of only one pawn able to sleep in Cabin when VREAndroids mod is installed
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.IsValidBedFor))]
    static class Patch_IsValidBedFor
    {
        static void Postfix(Thing bedThing, ref bool __result){
        }
    }
}
