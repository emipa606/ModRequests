using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.GeneCollectorQOL;

// draw total available archite capsules count in "Create Xenogerm" dialog
[HarmonyPatch(typeof(BiostatsTable), nameof(BiostatsTable.Draw))]
static class Patch_BiostatsTable {

    static bool gDrawMax;
    static int cacheTick = 0;
    static int cachedAvailCapsules = 0;

    static int CountAvailCapsules(){
        int tick = GenTicks.TicksGame;
        if ( cacheTick == tick )
            return cachedAvailCapsules;

        cacheTick = tick;
        cachedAvailCapsules = 0;

        List<Thing> list = Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.ArchiteCapsule);
        foreach (Thing item in list) {
            if (!item.Position.Fogged(Find.CurrentMap)) {
                cachedAvailCapsules += item.stackCount;
            }
        }

        return cachedAvailCapsules;
    }

    static bool? cachedModMoreGeneInfo;
    static bool hasModMoreGeneInfo(){
        if( cachedModMoreGeneInfo.HasValue ) return cachedModMoreGeneInfo.Value;

        cachedModMoreGeneInfo = ModLister.HasActiveModWithName("More Gene Information");
        return cachedModMoreGeneInfo.Value;
    }

    static void Prefix(bool drawMax, int maxGCX, ref bool ignoreLimits){
        gDrawMax = drawMax;

        // "More Gene Information" mod sets ignoreLimits to false thus disabling the max complexity vanilla indicator
        // return it back
        if( ModConfig.Settings.patchGeneAssembler_showCapsules && ignoreLimits && drawMax && hasModMoreGeneInfo() ){
            ignoreLimits = false;
        }
    }

    public static void Label(Rect rect, string label){
        if( gDrawMax && ModConfig.Settings.patchGeneAssembler_showCapsules ){
            int needed = Convert.ToInt32(label) * Patch_Dialog_CreateXenogerm.n;
            label = needed.ToString();
            int have = CountAvailCapsules();
            if( needed > have ){
                label = label.Colorize(ColorLibrary.RedReadable);
            }
            label = label + " / " + have;
        }
        Widgets.Label(rect, label);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        MethodInfo origLabel = HarmonyLib.AccessTools.Method( typeof(Widgets), "Label", new[] {typeof(Rect), typeof(string)});
        MethodInfo myLabel = HarmonyLib.AccessTools.Method( typeof(Patch_BiostatsTable), nameof(Patch_BiostatsTable.Label));

      var codes = instructions.ToList();
        for (int i = 0; i < codes.Count; i++){
            if( 
                    codes[i].opcode == OpCodes.Ldarga_S && Convert.ToInt32(codes[i].operand) == 3 &&
                    codes[i+1].opcode == OpCodes.Call && // System.String System.Int32::ToString()
                    codes[i+2].Calls(origLabel)
              ){
                codes[i+2].operand = myLabel;
                break;
            }
        }
        return codes;
    }
}
