using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace zed_0xff.GeneCollectorQOL;

// Random's Gene Assistant: support all genepacks
public static class Patch_RandomsGeneAssistant
{
    const string PackageId = "rimworld.randomcoughdrop.geneassistant";

    public static void HandlePatch(Harmony harmony) {
        if( !ModConfig.Settings.patchRGA ) return;
        if( !LoadedModManager.RunningModsListForReading.Any(m => m.PackageId == PackageId)) return;

        harmony.Patch(TargetMethod(),
                transpiler: new HarmonyMethod(
                    MethodBase.GetCurrentMethod().DeclaringType.GetMethod("Transpiler", BindingFlags.Static | BindingFlags.Public),
                    after: new[] { PackageId }
                    )
                );
    }

    public static MethodBase TargetMethod()
    {
        MethodInfo mi = AccessTools.Method(AccessTools.TypeByName("RandomsGeneAssistant.PatchTraderColorChange"), "DrawTradeableRow_Prefix");
        return mi;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo get_ThingDef = AccessTools.PropertyGetter(typeof(Transferable), nameof(Transferable.ThingDef));
        MethodInfo get_AnyThing = AccessTools.PropertyGetter(typeof(Transferable), nameof(Transferable.AnyThing));
        FieldInfo fGenepack = AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Genepack));

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++){
            if (
                    codes[i].Calls(get_ThingDef) &&
                    codes[i+1].LoadsField(fGenepack) &&
                    codes[i+2].opcode == OpCodes.Beq_S &&
                    codes[i+3].opcode == OpCodes.Ret
               ) {
                yield return new CodeInstruction(OpCodes.Callvirt, get_AnyThing);
                yield return new CodeInstruction(OpCodes.Isinst, typeof(Genepack));
                yield return new CodeInstruction(OpCodes.Brtrue, codes[i+2].operand);
                i+=2;
            } else {
                yield return codes[i];
            }
        }
    }
}
