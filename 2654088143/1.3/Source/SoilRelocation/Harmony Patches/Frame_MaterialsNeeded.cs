using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;

namespace SR
{
    [HarmonyPatch(typeof(Frame), "MaterialsNeeded")]
    internal static class Frame_MaterialsNeeded
    {
        internal static MethodInfo _harmonyPatchSharedData_GetWaterFillAdjustedCostListForFrame = AccessTools.Method(typeof(HarmonyPatchSharedData), "GetWaterFillAdjustedCostListForFrame");

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool fired = false;
            var instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                var instruction = instructionList[i];
                yield return instruction;
                if (!fired && instruction.opcode == OpCodes.Ldc_I4_1)
                {
                    ++i; //Skip next opcode, we're replacing it.
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //add "this" to the stack (for the Frame arg)
                    yield return new CodeInstruction(OpCodes.Call, _harmonyPatchSharedData_GetWaterFillAdjustedCostListForFrame); //Call our method instead that wraps theirs..
                    fired = true; //Only let it fire once.
                }
            }
        }
    }
}