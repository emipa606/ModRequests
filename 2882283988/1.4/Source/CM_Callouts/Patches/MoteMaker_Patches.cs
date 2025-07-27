using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class MoteMaker_Patches
    {
        [HarmonyPatch(typeof(MoteMaker))]
        [HarmonyPatch("ThrowText", new Type[] { typeof(Vector3), typeof(Map), typeof(string), typeof(Color), typeof(float) })]
        public static class MoteMaker_ThrowText
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo moteSpawner = AccessTools.Method(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(WipeMode) });
                MethodInfo newMoteSpawner = AccessTools.Method(typeof(CalloutTracker), "ThrowText", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(WipeMode) });

                bool replaceCall = true;
                foreach (CodeInstruction instruction in instructions)
                {
                    if (replaceCall && instruction.Calls(moteSpawner))
                    {
                        replaceCall = false;
                        instruction.operand = newMoteSpawner;
                    }

                    yield return instruction;
                }
            }
        }
    }
}
