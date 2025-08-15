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
    public static class HarmonyPatch_HatDisplaySelection
    {
        static FieldInfo pawnField = AccessTools.Field(AccessTools.TypeByName("HatDisplaySelection.Patch"), "pawn");
        static FieldInfo headFacingField = AccessTools.Field(AccessTools.TypeByName("HatDisplaySelection.Patch"), "headFacing_");
        internal static readonly MethodInfo DrawLocAndQuatMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new Type[]
        {//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool),
        });
        internal static IEnumerable<CodeInstruction> DrawApparel2_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Apparel apparel, Rot4 headFacing)
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparelHeadBody));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawHatCEWithHair_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Apparel apparel, Rot4 headFacing)
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldsfld, headFacingField);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparelHeadBody));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawHatWithHair_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Apparel apparel, Rot4 headFacing)
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 3);
                    yield return new CodeInstruction(OpCodes.Ldsfld, headFacingField);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparelHeadBody));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawHatWithHair_14_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Apparel apparel, Rot4 headFacing)
            int step = 0;
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    if (step == 0)
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustFaceTatoo));
                    }
                    else if (step == 1)
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustBeard));
                    }
                    else if (step == 2)
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHair));
                    }
                    step++;
                    continue;
                }
                yield return code;
            }
        }
        public static Type nestedType;
        internal static IEnumerable<CodeInstruction> DrawHeadHairCompiled_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//IL_000b: ldarg.2 IL_000c: ldfld valuetype Verse.Rot4 Verse.PawnRenderer / '<>c__DisplayClass39_0'::headFacing

            bool apparelHead = false;
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(nestedType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(nestedType, "flags"));
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparelHead));
                    apparelHead = true;
                    continue;
                }
                yield return code;
            }
            if (!apparelHead) Log.Warning($"[Stylish Rim] Failed adjust apparel head transpiler.");
        }
        internal static IEnumerable<CodeInstruction> DrawExtraEyesCompiled_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(HarmonyPatch_PawnRender.DrawMatrixMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(nestedType, "headFacing"));
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHeadExtraEyes));
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> DrawHeadGenesCompiled_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(HarmonyPatch_PawnRender.DrawMatrixMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(nestedType, "headFacing"));
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHeadGenes));
                    continue;
                }
                yield return code;
            }
        }
    }
}
