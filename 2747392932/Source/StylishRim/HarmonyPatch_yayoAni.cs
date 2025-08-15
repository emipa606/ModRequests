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
    public static class HarmonyPatch_yayoAni
    {
        internal static IEnumerable<CodeInstruction> DrawEquipmentAiming_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            MethodInfo DrawMesh = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new System.Type[]
            {// Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(int)
            });
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart_yayoAni));
                    continue;
                }
                yield return code;
            }
            */
            int step = 0;
            MethodInfo Matrix4x4TRS = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.TRS));
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(Matrix4x4TRS))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart_yayoAni));
                    step++;
                    continue;
                }
                else if (step == 1 && code.opcode == OpCodes.Ldloc_S)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart1_4FlipMesh));
                    step++;
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> Prefix_AlienRace_HarmonyPatches_DrawAddons_Prefix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation
            // アドオンの各種補正をかける処理
            string stLocS7OperandString = "UnityEngine.Vector3 (5)";
            bool completeOffset = false;
            foreach (CodeInstruction code in instructions)
            {
                if (!completeOffset && OpCodes.Stloc_S == code.opcode && stLocS7OperandString == code.operand.ToString())
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AddAddonOffset));
                    completeOffset = true;
                }
                else if (code.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustAddons));
                    continue;
                }
                /*
                else if (code.opcode == OpCodes.Stloc_3)
                {
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(NoPortrait));
                }
                */
                yield return code;
            }
        }
    }
}
