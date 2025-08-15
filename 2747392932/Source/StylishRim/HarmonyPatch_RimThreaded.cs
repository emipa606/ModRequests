using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StylishRim
{
    public static class HarmonyPatch_RimThreaded
    {
        internal static IEnumerable<CodeInstruction> Get_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // ポートレート描画に修正かけるやつ
            // 別MODと競合せずにリアルタイムで変動させるにはTranspilerで頭に処理ぶっこむしかなかった
            List<CodeInstruction> codes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarga, 3),
                new CodeInstruction(OpCodes.Ldarga, 4),
                new CodeInstruction(OpCodes.Ldarga, 5),
                new CodeInstruction(OpCodes.Ldarga, 8),
                new CodeInstruction(OpCodes.Ldarga, 9),
                new CodeInstruction(OpCodes.Ldarg, 12),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustPortraitOffset)))
            };
            codes.AddRange(instructions);
            return codes;
        }
    }
}
