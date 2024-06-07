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

// show messages for gene spawner genes
public static class Patch_WVC_GeneSpawner
{
    const string PackageId = "wvc.sergkart.races.biotech";

    public static void HandlePatch(Harmony harmony) {
        if( !ModConfig.Settings.patchWVC ) return;
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
        MethodInfo mi = AccessTools.Method(AccessTools.TypeByName("WVC_XenotypesAndGenes.Gene_Spawner"), "SpawnItems");
        return mi;
    }

    static MethodInfo myShowMsg = HarmonyLib.AccessTools.Method(
            typeof(Patch_WVC_GeneSpawner),
            nameof(Patch_WVC_GeneSpawner.showMsg)
            );

    public static float sat => 1.0f;
    public static Color yellow => new Color(sat, sat, 0);
    public static Color green  => new Color(0,   sat, 0);
    public static Color red    => new Color(sat, 0,   0);

    static void showMsg(Pawn pawn, Thing thing){
        if( !ModConfig.Settings.patchWVC ) return;

        if( thing is Genepack gp){
            string text = gp.GeneSet.GenesListForReading.Select((Func<GeneDef, string>)(x => x.label)).ToCommaList().CapitalizeFirst();
            switch( GeneCache.GenepackStatus(gp) ){
                case 0:
                    text = null;
                    break;
                case 1:
                    text = text.Colorize(red);
                    break;
                case 2:
                    text = text.Colorize(yellow);
                    break;
            }
            if( text != null ){
                Messages.Message(
                        (string)("GenepackSpawned".Translate(pawn.Named("PAWN"), thing.def.label)) + text,
                        new LookTargets((TargetInfo)pawn, (TargetInfo)gp),
                        MessageTypeDefOf.PositiveEvent);
            }
        }
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++){
            if( codes[i].opcode == OpCodes.Ret && i == codes.Count - 1 ){
                yield return new CodeInstruction(OpCodes.Ldarg_1, null); // Pawn
                yield return new CodeInstruction(OpCodes.Ldloc_0, null); // Genepack
                yield return new CodeInstruction(OpCodes.Call, myShowMsg);
            }
            yield return codes[i];
        }
    }
}
