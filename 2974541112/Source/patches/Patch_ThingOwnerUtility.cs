using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// TSS: set comfort temperature for pawns/things inside
[HarmonyPatch(typeof(ThingOwnerUtility), nameof(ThingOwnerUtility.TryGetFixedTemperature))]
static class Patch_TryGetFixedTemperature
{
    static bool Prefix( ref bool __result, ref float temperature, IThingHolder holder, Thing forThing ){
        if( holder is Building_TSS tss && tss.PowerOn ){
            temperature = ( forThing is Pawn ) ? 21f : 0f;
            __result = true;
            return false;
        }
        return true;
    }
}

// TSS: mark contents as in cryptosleep if time is freezed
[HarmonyPatch(typeof(ThingOwnerUtility), nameof(ThingOwnerUtility.ContentsInCryptosleep))]
static class Patch_ContentsInCryptosleep
{
    static void Postfix( ref bool __result, IThingHolder holder ){
        if( holder is Building_TSS tss && tss.IsContentsSuspended ){
            __result = true;
        }
    }
}
