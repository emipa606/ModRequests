using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StylishRim
{
    public static class HarmonyPatch_ShowHair
    {
        internal static IEnumerable<CodeInstruction> DrawMeshNowOrLaterPatch_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                /*
                if (code.OperandIs(GetAlienHeadMeshMethod))
                {
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.BeginDrawHair));
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.EndDrawHair));
                    continue;
                }
                else if (code.opcode == OpCodes.Ldfld && code.operand != null)
                {
                    if (code.operand.ToString().Contains("onHeadLoc"))
                    {
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return CodeInstruction.Call(typeof(PawnRenderFlagsExtension), nameof(PawnRenderFlagsExtension.FlagSet));
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustHairOffset));
                        continue;
                    }
                }
                */
                if (code.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(AccessTools.TypeByName("ShowHair.Patch_PawnRenderer_DrawHeadHair"), "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(AccessTools.TypeByName("ShowHair.Patch_PawnRenderer_DrawHeadHair"), "flags"));
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHair));
                    continue;
                }
                yield return code;
            }
        }
    }
}