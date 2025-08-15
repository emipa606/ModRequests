using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace StylishRim
{
    internal class HarmonyPatch_PawnCacheRenderer
    {
        internal static IEnumerable<CodeInstruction> Get_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // ポートレート描画に修正かけるやつ
            // 別MODと競合せずにリアルタイムで変動させるにはTranspilerで頭に処理ぶっこむしかなかった
            List<CodeInstruction> codes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarga, 2),
                new CodeInstruction(OpCodes.Ldarga, 3),
                new CodeInstruction(OpCodes.Ldarga, 4),
                new CodeInstruction(OpCodes.Ldarga, 7),
                new CodeInstruction(OpCodes.Ldarga, 8),
                new CodeInstruction(OpCodes.Ldarg, 11),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustPortraitOffset)))
            };
            codes.AddRange(instructions);
            return codes;
        }

        internal static IEnumerable<CodeInstruction> RenderPawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // ポートレートの角度調整
            // こればかりはリアルタイム変動できねえだ
            int step = 0;
            MethodInfo GetPosition = AccessTools.PropertyGetter(typeof(UnityEngine.Transform), nameof(UnityEngine.Transform.position));
            MethodInfo SetPosition = AccessTools.PropertySetter(typeof(UnityEngine.Transform), nameof(UnityEngine.Transform.position));
            foreach (CodeInstruction code in instructions)
            {
                if (step == 0 && code.OperandIs(GetPosition))
                {
                    step++;
                }
                else if (step == 1 && code.OperandIs(GetPosition))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return code;
                    continue;
                }
                else if (step == 1 && code.OperandIs(SetPosition))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg, 11);
                    yield return new CodeInstruction(OpCodes.Ldarg, 15);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustPortraitAngle)));
                    yield return new CodeInstruction(OpCodes.Dup);
                    step++;
                    continue;
                }
                else if (step == 2 && code.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg, 15);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.RemovePortraitAngle)));
                }
                yield return code;
            }
        }
    }
}
