using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreRecordsInteraction {
    [StaticConstructorOnStartup]
    public class HarmonyPatches {
        static HarmonyPatches() {
            var harmony = new Harmony("com.tammybee.morerecords.interaction");
            harmony.PatchAll();

            foreach (RecordDef def in InteractionRecordDefGenerator.ImpliedRecordDefs()) {
                DefGenerator.AddImpliedDef<RecordDef>(def);
            }
        }
    }

    /*
    [HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve")]
    public class DefGenerator_GenerateImpliedDefs_PreResolve_Patch {
        [HarmonyPostfix]
        static void Postfix() {
            foreach (RecordDef def in InteractionRecordDefGenerator.ImpliedRecordDefs()) {
                DefGenerator.AddImpliedDef<RecordDef>(def);
            }
        }
    }
    */

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public class Pawn_InteractionsTracker_TryInteractWith_Patch {
        static void Postfix(bool __result, Pawn ___pawn, Pawn recipient, InteractionDef intDef) {
            if (__result) {
                if (___pawn != null) {
                    ___pawn.records.Increment(InteractionRecordDefGenerator.GetInteractRecordDef(intDef));
                }
                if (recipient != null) {
                    recipient.records.Increment(InteractionRecordDefGenerator.GetInteractedRecordDef(intDef));
                }
            }
        }
    }
}
