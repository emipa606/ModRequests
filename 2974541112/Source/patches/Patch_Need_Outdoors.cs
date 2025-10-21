using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

// TSS: make them feel inside
[HarmonyPatch(typeof(Need_Outdoors), nameof(Need_Outdoors.NeedInterval))]
static class Patch__Need_Outdoors__NeedInterval
{

    private static bool IsDisabled(Pawn pawn) {
        if (!pawn.Dead) {
            return !pawn.needs.EnjoysOutdoors();
        }
        return true;
    }

    static bool Prefix(ref Need_Outdoors __instance, ref Pawn ___pawn, ref float ___lastEffectiveDelta){
        if( ___pawn.Spawned || IsDisabled(___pawn) )
            return true;

        if( ___pawn.ParentHolder is Building_TSS tss && !tss.IsContentsSuspended ){
            float curLevel = __instance.CurLevel;
            const float num = -0.005f;
            __instance.CurLevel = Mathf.Max(__instance.CurLevel + num, 0);
            ___lastEffectiveDelta = __instance.CurLevel - curLevel;
            return false;
        }

        return true;
    }
}
