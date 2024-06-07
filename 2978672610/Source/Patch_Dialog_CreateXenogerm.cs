using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace zed_0xff.GeneCollectorQOL;

static class Patch_Dialog_CreateXenogerm {
    public static int n = 1;

    public static string TicksToPeriod(int numTicks, bool allowSeconds = true, bool shortForm = false, bool canUseDecimals = true, bool allowYears = true, bool canUseDecimalsShortForm = false){
        return (numTicks*n).ToStringTicksToPeriod(allowSeconds, shortForm, canUseDecimals, allowYears, canUseDecimalsShortForm);
    }

    [HarmonyPatch(typeof(Dialog_CreateXenogerm), nameof(Dialog_CreateXenogerm.PostOpen))]
    class Patch_PostOpen {
        static void Postfix(){
            n = 1;
        }
    }

    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "StartAssembly")]
    class Patch_StartAssembly {
        static void Postfix(Building_GeneAssembler ___geneAssembler){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;
            Patch_Building_GeneAssembler.queues[___geneAssembler.thingIDNumber] = n-1;
            n = 1;
        }
    }

    [HarmonyPatch(typeof(Dialog_CreateXenogerm), "DoBottomButtons")]
    class Patch_DoBottomButtons {
        static void Postfix(Dialog_CreateXenogerm __instance, List<Genepack> ___selectedGenepacks, Rect rect, int ___arc, Vector2 ___ButSize){
            if( !ModConfig.Settings.patchGeneAssembler_addQueue ) return;

            string text = n + "x";
            var textSize = Text.CalcSize(text);
            var prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.65f);
            GenUI.SetLabelAlign(TextAnchor.LowerRight);
            Widgets.Label(new Rect(rect.xMax - 16-20, rect.y-20-textSize.y, 16+20, textSize.y), text );
            GenUI.ResetLabelAlign();
            GUI.color = prevColor;

            if( Widgets.ButtonImage(new Rect(rect.xMax - 16-20, rect.y-20, 16, 16), TexButton.Minus ) ){
                n -= GenUI.CurrentAdjustmentMultiplier();
                if( n < 1 ) n=1;
            }
            if( Widgets.ButtonImage(new Rect(rect.xMax - 16, rect.y-20, 16, 16), TexButton.Plus ) ){
                n += GenUI.CurrentAdjustmentMultiplier();
                if( n > 99 ) n=99;
            }
        }


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo origTicks = HarmonyLib.AccessTools.Method( typeof(GenDate), nameof(GenDate.ToStringTicksToPeriod) );
            MethodInfo myTicks = HarmonyLib.AccessTools.Method( typeof(Patch_Dialog_CreateXenogerm), nameof(Patch_Dialog_CreateXenogerm.TicksToPeriod) );

            foreach( CodeInstruction code in instructions ){
                if( code.Calls(origTicks) ){
                    code.operand = myTicks;
                }
                yield return code;
            }
        }
    }
}
