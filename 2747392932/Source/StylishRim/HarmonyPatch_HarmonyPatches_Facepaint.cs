using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_HarmonyPatches_Facepaint
    {
        internal static readonly MethodInfo DrawLocAndQuatMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new System.Type[]
        {//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool),
        });
        [HarmonyPriority(401)]
        internal static IEnumerable<CodeInstruction> RenderFacepaint_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHead));
                    continue;
                }
                yield return code;
            }
        }
    }
}
