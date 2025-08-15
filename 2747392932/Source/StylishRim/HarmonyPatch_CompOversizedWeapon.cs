using AlienRace;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_CompOversizedWeapon
    {

        public static MethodInfo DrawMeshMatrix = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new System.Type[]
        {// Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer
                typeof(Mesh),
                typeof(Matrix4x4),
                typeof(Material),
                typeof(int)
        });

        internal static IEnumerable<CodeInstruction> DrawEquipmentAimingPreFix_Transpiler(IEnumerable<CodeInstruction> codes)
        {
            List<CodeInstruction> locAndQuat = new List<CodeInstruction>();
            MethodInfo setTRSMethod = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.SetTRS));
            MethodInfo TRSMethod = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.TRS));
            string loc10String = "UnityEngine.Matrix4x4 (10)";

            bool inMatrix = false;
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Ldarg_2)
                {
                    inMatrix = true;
                    locAndQuat.Clear();
                }
                else if (code.OperandIs(setTRSMethod) || code.OperandIs(TRSMethod))
                {
                    inMatrix = false;
                    continue;
                }
                else if ((code.opcode == OpCodes.Ldloca_S || code.opcode == OpCodes.Stloc_S) && code.operand.ToString().Contains(loc10String))
                {
                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(code.labels);
                    continue;
                }
                else if (code.opcode == OpCodes.Ldloc_S && code.operand.ToString().Contains(loc10String))
                {
                    yield return new CodeInstruction(OpCodes.Nop).WithLabels(code.labels);
                    foreach (CodeInstruction c in locAndQuat)
                    {
                        yield return c;
                    }
                    continue;
                }
                else if (code.OperandIs(DrawMeshMatrix))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPartWithScale));
                    continue;
                }

                if (inMatrix)
                {
                    locAndQuat.Add(code);
                }
                else
                {
                    yield return code;
                }
            }
        }
    }
}
