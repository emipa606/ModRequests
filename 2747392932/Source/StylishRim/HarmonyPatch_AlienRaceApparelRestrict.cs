using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace StylishRim
{
    // AlienRaceの処理に寄生するためのパッチ集。
    public static class HarmonyPatch_AlienRaceApparelRestrict
    {
        public static IEnumerable<CodeInstruction> CanEquipPostfix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ldind_U1)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRaceApparelRestrict), nameof(HarmonyPatch_AlienRaceApparelRestrict.IsDefaultRestriction));
                    continue;
                }
                yield return code;
            }
        }

        public static bool IsDefaultRestriction(bool result, Pawn pawn)
        {
            return result && !(PawnStyleSet.Get(pawn)?.IgnoreRaceRestriction ?? false);
        }
    }
}
