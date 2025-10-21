using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// TSS allow mental states when unpowered
[HarmonyPatch]
static class Patch_MentalStateWorker {

    static MethodInfo origSpawned = HarmonyLib.AccessTools.Method(typeof(Thing), "get_Spawned");
    static MethodInfo mySpawned   = HarmonyLib.AccessTools.Method(typeof(Patch_MentalStateWorker), "Spawned");

    static MethodInfo origMap     = HarmonyLib.AccessTools.Method(typeof(Thing), "get_Map");
    static MethodInfo myMap       = HarmonyLib.AccessTools.Method(typeof(Patch_MentalStateWorker), "Map");

    static IEnumerable<MethodBase> TargetMethods() {
        var m0 = AccessTools.Method(typeof(MentalStateWorker), nameof(MentalStateWorker.StateCanOccur));
        yield return m0;

        foreach( var t in typeof(MentalStateWorker).AllSubclassesNonAbstract() ){
            var m = AccessTools.Method(t, "StateCanOccur");
            if( m != null && m != m0 ){
                yield return m;
            }
        }
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        foreach (var code in instructions){
            if( code.opcode == OpCodes.Callvirt ){
               if( (MethodInfo)code.operand == origSpawned ){
                   code.operand = mySpawned;
               } else if( (MethodInfo)code.operand == origMap ){
                   code.operand = myMap;
               }
            }
            yield return code;
        }
    }

    // disable some states back again
    static void Postfix( ref MentalStateWorker __instance, ref bool __result, Pawn pawn ){
        if( __result && pawn != null && pawn.ParentHolder is Building_TSS tss && tss.IsInternalBatteryEmpty() ){
            if( 
                    __instance.def == MentalStateDefOf.Roaming ||
                    __instance.def == MentalStateDefOf.Wander_OwnRoom ||
                    __instance.def == MentalStateDefOf.Wander_Psychotic ||
                    __instance.def == MentalStateDefOf.Wander_Sad
              ){
                __result = false;
            }
        }
    }

    public static Map Map(Thing thing) {
        Map result = thing.Map;
        if( result == null && thing is Pawn pawn && pawn.ParentHolder is Building_TSS tss && tss.IsInternalBatteryEmpty() ){
            result = tss.Map;
        }
        return result;
    }

    public static bool Spawned(Thing thing) {
        bool result = thing.Spawned;
        if( !result && thing is Pawn pawn && pawn.ParentHolder is Building_TSS tss && tss.IsInternalBatteryEmpty() ){
            result = true;
        }
        return result;
    }
}
