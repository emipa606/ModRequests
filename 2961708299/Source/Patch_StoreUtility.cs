using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace zed_0xff.LoftBed {

    // fix vanilla shelves stop accepting items when placed under a LoftBed
    [HarmonyPatch(typeof(StoreUtility), "NoStorageBlockersIn")]
    static class Patch_NoStorageBlockersIn
    {
        static MethodInfo origThingsListAt = HarmonyLib.AccessTools.Method(typeof(ThingGrid), "ThingsListAt");
        static MethodInfo myThingsListAt   = HarmonyLib.AccessTools.Method(typeof(Patch_NoStorageBlockersIn), "FilteredThingsListAt");

        private static List<Thing> FilteredThingsListAt(ThingGrid grid, IntVec3 c){
            List<Thing> list = grid.ThingsListAt(c);
            for(int i=0; i<list.Count; i++){
                if( BedCache.isLoftBed(list[i]) ){
                    return list.Where((Thing t) => !BedCache.isLoftBed(t)).ToList();
                }
            }
            return list;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions){
                // callvirt System.Collections.Generic.List`1<Verse.Thing> Verse.ThingGrid::ThingsListAt(Verse.IntVec3 c)
                if ( code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == origThingsListAt ){
                    code.operand = myThingsListAt;
                }
                yield return code;
            }
        }
    }
}
