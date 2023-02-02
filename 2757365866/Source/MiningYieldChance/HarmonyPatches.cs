using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace MiningYieldChance
{
    [HotSwappable]
    [HarmonyPatch(typeof(Mineable), nameof(Mineable.TrySpawnYield))]
    public static class MultiplyMineableDropChance
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            var target = AccessTools.Field(typeof(BuildingProperties), nameof(BuildingProperties.mineableDropChance));
            var ourMethod = AccessTools.Method(typeof(MultiplyMineableDropChance), nameof(GetChance));

            foreach (var ci in instr)
            {
                yield return ci;

                if (ci.opcode == OpCodes.Ldfld && ci.operand is FieldInfo field && field == target)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, (short)4);
                    yield return new CodeInstruction(OpCodes.Call, ourMethod);
                    yield return new CodeInstruction(OpCodes.Add);
                }
            }
        }

        private static float GetChance(Pawn pawn) => pawn?.GetStatValue(MiningYieldStatDefOf.MiningChunkDropChance) ?? 1.0f;
    }
}
