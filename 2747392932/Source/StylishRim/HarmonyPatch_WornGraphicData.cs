using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace StylishRim
{
    public static class HarmonyPatch_WornGraphicData
    {
        public static readonly List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>()
        {
            BodyTypeDefOf.Male,
            BodyTypeDefOf.Female,
            BodyTypeDefOf.Thin,
            BodyTypeDefOf.Hulk,
            BodyTypeDefOf.Fat
        };
        internal static IEnumerable<CodeInstruction> BeltOffsetAt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_WornGraphicData), nameof(HarmonyPatch_WornGraphicData.ReBeltOffsetAt));
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> BeltScaleAt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_WornGraphicData), nameof(HarmonyPatch_WornGraphicData.ReBeltScaleAt));
                }
                yield return code;
            }
        }

        public static Vector2 ReBeltOffsetAt(Vector2 offset, BodyTypeDef bodyType, WornGraphicDirectionData wornGraphicDirectionData)
        {
            if (bodyTypes.Contains(bodyType)) return offset;
            return offset + wornGraphicDirectionData.female.offset;
        }
        public static Vector2 ReBeltScaleAt(Vector2 offset, BodyTypeDef bodyType, WornGraphicData wornGraphicData)
        {
            if (bodyTypes.Contains(bodyType)) return offset;
            return wornGraphicData.female.Scale;
        }
    }
}
