using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UnlimitedPerspectName
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.erdelf.Unlimited PreceptName");
            Harmony.DEBUG = false;
            harmony.PatchAll();
        }
    }


    [HarmonyPatch(typeof(Dialog_EditPrecept), "DoWindowContents")]
    public static class UnlimitedPreceptName
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Ldc_I4_S && code.OperandIs(32))
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue);
                }
                else
                {
                    yield return code;
                }
            }

        }
    }

//Ignore the clusterfuck below , It works , I don't want to worry about it
[HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), "DoWindowContents")]
public static class UnlimitedSymboleName{ static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
{int i = 0; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
var codes = new List<CodeInstruction>(instructions); foreach (var code in codes) { if ((code.opcode == OpCodes.Ldc_I4_S) && code.OperandIs(40))
{yield return new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue); } else { yield return code;}i++; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; }}}

//    [HarmonyPatch(typeof(Dialog_ChooseIdeoSymbols), "DoWindowContents")]
//    public static class UnlimitedSymbolName
//    {
//        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
//        {
//            var codes = instructions.ToList();
            //var UnsureWhatName = AccessTools.Field(typeof(Dialog_ChooseIdeoSymbols), nameof(Dialog_ChooseIdeoSymbols.newName));
            // && codes[i-1].LoadsField(UnsureWhatName)
//            for (var i = 0; i < codes.Count; i++)
//            {
//                if (i > 2 && codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].OperandIs(40))
//                {
//                    yield return new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue);
//                }
//                else { yield return codes[i]; }
//
//            }
//        }
//    }

}
