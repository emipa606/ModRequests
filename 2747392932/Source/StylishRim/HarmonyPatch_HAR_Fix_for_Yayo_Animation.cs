using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_HAR_Fix_for_Yayo_Animation
    {
        internal static IEnumerable<CodeInstruction> Prefix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // アドオンの描画に修正かかってるので再寄生

            FieldInfo getStfldDrawSize = AccessTools.Field(typeof(Graphic), nameof(Graphic.drawSize));
            MethodInfo isPortraitFlag = AccessTools.Method(typeof(PawnRenderFlagsExtension), nameof(PawnRenderFlagsExtension.FlagSet));
            string stLocS7OperandString = "UnityEngine.Vector3 (7)";
            bool completeOffset = false;

            foreach (CodeInstruction code in instructions)
            {
                if (!completeOffset && OpCodes.Stloc_S == code.opcode && stLocS7OperandString == code.operand.ToString())
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.AddAddonOffset)));
                    completeOffset = true;
                }
                else if (code.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Call, isPortraitFlag);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustAddons));
                    continue;
                }
                /*
                else if (code.opcode == OpCodes.Stfld && code.OperandIs(getStfldDrawSize))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Call, isPortraitFlag);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustAddonDrawSize)));
                }
                else if (code.opcode == OpCodes.Stloc_1)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.SetUpAddon));
                    continue;
                }
                */
                yield return code;
            }
        }
    }
}