using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StylishRim
{
    internal class HarmonyPatch_DualWield
    {
        internal static IEnumerable<CodeInstruction> Prefix_Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Or)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldind_Ref);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DynamicPartDualWield));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawEquipmentAimingOverride_Transpiler(IEnumerable<CodeInstruction> codes)
        {
            MethodInfo DrawMesh = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new System.Type[]
            {// Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(int)
            });
            foreach (CodeInstruction code in codes)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPartOffHand));
                    continue;
                }
                yield return code;
            }
        }
    }
}
