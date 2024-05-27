using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using VNPE;

namespace zed_0xff.VNPE_Fridge_Fix {
    [HarmonyPatch(typeof(Building_NutrientGrinder), "HasEnoughFeed")]
    static class Patch_HasEnoughFeed
    {
// can remove 'break' with a transpiler, but we also need to process all the hopper cells (
//        static MethodInfo mGetStatValue = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));
//
//        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
//            CodeInstruction[] iarr = instructions.ToArray();
//            // call static System.Single RimWorld.StatExtension::GetStatValue
//            // mul NULL
//            // add NULL
//            // stloc.1 NULL
//            // br.s Label3
//            // nop NULL [Label2]
//            for( int i=0; i<iarr.Count(); i++ ){
//                if( iarr[i].opcode == OpCodes.Call && iarr[i].operand as MethodInfo == mGetStatValue &&
//                        iarr[i+1].opcode == OpCodes.Mul &&
//                        iarr[i+2].opcode == OpCodes.Add &&
//                        iarr[i+3].opcode == OpCodes.Stloc_1 &&
//                        iarr[i+4].opcode == OpCodes.Br_S &&
//                        iarr[i+5].opcode == OpCodes.Nop
//                  ){
//                    iarr[i+4] = new CodeInstruction(OpCodes.Nop);
//                }
//            }
//            return iarr;
//        }

        static bool Postfix(bool __result, Building_NutrientGrinder __instance, List<Thing> ___cachedHoppers){
            if( __result ) return __result;

            var map = __instance.Map;
            var num = 0f;

            for (int h = 0; h < ___cachedHoppers.Count; h++)
            {
                // fix #1: process all cells of hopper, mostly for 2x1 fridges, but 2x2 will work too :)
                foreach( var cell in ___cachedHoppers[h].OccupiedRect() ){
                    var things = cell.GetThingList(map);

                    for (int t = 0; t < things.Count; t++)
                    {
                        var thing = things[t];
                        if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(thing.def))
                        {
                            num += thing.stackCount * thing.GetStatValue(StatDefOf.Nutrition);
                            // fix #2: replace break with sufficiency check
                            if (num >= __instance.def.building.nutritionCostPerDispense)
                                return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Building_NutrientGrinder), "FindFeedInAnyHopper")]
    static class Patch_FindFeedInAnyHopper
    {
        static Thing Postfix(Thing __result, Building_NutrientGrinder __instance, List<Thing> ___cachedHoppers){
            if( __result != null ) return __result;

            for (int h = 0; h < ___cachedHoppers.Count; h++)
            {
                // fix #1: process all cells of hopper, mostly for 2x1 fridges, but 2x2 will work too :)
                foreach( var cell in ___cachedHoppers[h].OccupiedRect() ){
                    var thingList = cell.GetThingList(__instance.Map);

                    for (int t = 0; t < thingList.Count; ++t)
                    {
                        Thing thing = thingList[t];
                        if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(thing.def))
                            return thing;
                    }
                }
            }
            return null;
        }
    }
}
