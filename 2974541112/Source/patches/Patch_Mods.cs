using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace zed_0xff.CPS;

// TSS: patch VFECore mod gene spawners to continue spawining items when pawn is inside the TSS
[HarmonyPatch]
static class Patch_Mods {

    static readonly Dictionary<MethodInfo, MethodInfo> replaceMethods = new Dictionary<MethodInfo, MethodInfo> {
        { AccessTools.Method(typeof(Thing), "get_Spawned"),
            AccessTools.Method(typeof(Patch_Mods), "Spawned") },
        { AccessTools.Method(typeof(Thing), "get_Map"),
            AccessTools.Method(typeof(Patch_Mods), "Map") },
        { AccessTools.Method(typeof(GenAdj), "CellsAdjacent8Way", new[]{ typeof(Thing) }),
            AccessTools.Method(typeof(Patch_Mods), "CellsAdjacent8Way", new[]{ typeof(Thing) }) },
    };

    // it is different from Patch_MentalStateWorker.cs !
    public static Map Map(Thing thing) {
        Map result = thing.Map;
        if( result == null && thing is Pawn pawn && pawn.ParentHolder is Building_TSS tss && !tss.IsContentsSuspended ){
            result = tss.Map;
        }
        if( result == null && thing is Corpse corpse && corpse.ParentHolder is Building_TSS tss2 && !tss2.IsContentsSuspended ){
            result = tss2.Map;
        }
        return result;
    }

    // it is different from Patch_MentalStateWorker.cs !
    public static bool Spawned(Thing thing) {
        bool result = thing.Spawned;
        if( !result && thing is Pawn pawn && pawn.ParentHolder is Building_TSS tss && !tss.IsContentsSuspended ){
            result = true;
        }
        return result;
    }

    public static IEnumerable<IntVec3> CellsAdjacent8Way(Thing thing) {
        if( thing is Pawn pawn && pawn.ParentHolder is Building_TSS tss && !tss.IsContentsSuspended ){
            return GenAdj.CellsAdjacent8Way(tss);
        } else {
            return GenAdj.CellsAdjacent8Way(thing);
        }
    }

    static IEnumerable<MethodBase> TargetMethods() {
        MethodInfo m;
        Type t;

        // [Alpha Genes](https://steamcommunity.com/sharedfiles/filedetails/?id=2891845502)
        t = AccessTools.TypeByName("AlphaGenes.HediffComp_Parasites");
        if( t != null ){
            m = AccessTools.Method(t, "Notify_PawnDied");
            if( m != null ){
                yield return m;
            }
        }

        // [Vanilla Expanded Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=2023507013)
        t = AccessTools.TypeByName("AnimalBehaviours.HediffComp_Spawner");
        if( t != null ){
            m = AccessTools.Method(t, "TickInterval");
            if( m != null ){
                yield return m;
            }

            m = AccessTools.Method(t, "TryDoSpawn");
            if( m != null ){
                yield return m;
            }

            m = AccessTools.Method(t, "TryFindSpawnCell");
            if( m != null ){
                yield return m;
            }
        }
        t = AccessTools.TypeByName("VanillaGenesExpanded.HediffComp_HumanEggLayer");
        if( t != null ){
            m = AccessTools.Method(t, "CompPostTick");
            if( m != null ){
                yield return m;
            }

            m = AccessTools.Method(t, "ProduceEgg");
            if( m != null ){
                yield return m;
            }
        }

// Error in static constructor of zed_0xff.CPS.Init: System.TypeInitializationException:
// The type initializer for 'zed_0xff.CPS.Init' threw an exception. --->
// HarmonyLib.HarmonyException: Patching exception in method virtual System.Void
// AnimalBehaviours.HediffComp_AsexualReproduction::CompPostTick(System.Single& severityAdjustment) ---> 
// System.FormatException: Method virtual System.Void AnimalBehaviours.HediffComp_AsexualReproduction::CompPostTick(System.Single& severityAdjustment)
// cannot be patched.
// Reason: Invalid IL code in (wrapper dynamic-method) AnimalBehaviours.HediffComp_AsexualReproduction:AnimalBehaviours.HediffComp_AsexualReproduction.CompPostTick_Patch0 (AnimalBehaviours.HediffComp_AsexualReproduction,single&):
// IL_06b2: ret
//
//        t = AccessTools.TypeByName("AnimalBehaviours.HediffComp_AsexualReproduction");
//        if( t != null ){
//            m = AccessTools.Method(t, "CompPostTick");
//            if( m != null ){
//                yield return m;
//            }
//        }
//        t = AccessTools.TypeByName("AnimalBehaviours.CompAsexualReproduction");
//        if( t != null ){
//            m = AccessTools.Method(t, "CompTick");
//            if( m != null ){
//                yield return m;
//            }
//        }

        // [Vanilla Factions Expanded - Insectoids](https://steamcommunity.com/sharedfiles/filedetails/?id=2149755445)
        t = AccessTools.TypeByName("VFEI.Comps.HediffComps.CompSpawnJelly");
        if( t != null ){
            m = AccessTools.Method(t, "CompPostTick");
            if( m != null ){
                yield return m;
            }
        }

        // [WVC - Xenotypes and Genes](https://steamcommunity.com/sharedfiles/filedetails/?id=2886992038)
        t = AccessTools.TypeByName("WVC.Gene_Spawner");
        if( t != null ){
            m = AccessTools.Method(t, "<GetGizmos>b__10_0");
            if( m != null ){
                yield return m;
            }

            m = AccessTools.Method(t, "Tick");
            if( m != null ){
                yield return m;
            }

            m = AccessTools.Method(t, "SpawnItems");
            if( m != null ){
                yield return m;
            }
        }
        t = AccessTools.TypeByName("WVC.HediffComp_Gestator");
        if( t != null ){
            m = AccessTools.Method(t, "CompPostTick");
            if( m != null ){
                yield return m;
            }
        }

        // don't yell there's nothing to patch if no other mods are there :)
        yield return AccessTools.Method( typeof(Patch_Mods), nameof(Patch_Mods.Stub) );
    }

    public static void Stub(){
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        foreach (var code in instructions){
            if( code.opcode == OpCodes.Callvirt || code.opcode == OpCodes.Call ){
                var m0 = code.operand as MethodInfo;
                if( m0 == null ){
                    // ctor
                    continue;
                }

                if( replaceMethods.TryGetValue(m0, out MethodInfo m1) ){
                    if( m1 == null ){
                        Log.Warning("[?] NULL replacement method for " + code);
                        continue;
                    }
                    code.operand = m1;
                }
            }
            yield return code;
        }
    }
}
