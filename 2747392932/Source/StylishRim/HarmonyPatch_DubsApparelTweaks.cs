using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_DubsApparelTweaks
    {
        internal static readonly MethodInfo DrawLocAndQuatMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new System.Type[]
        {//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool),
        });
        internal static IEnumerable<CodeInstruction> Prefix_Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (CodeInstruction code in codes)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHair)));
                    continue;
                }
                yield return code;
            }
        }
    }
}
