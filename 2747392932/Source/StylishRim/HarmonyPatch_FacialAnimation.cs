using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace StylishRim
{
    // Facial Animationへの寄生メソ
    // パーツごとに位置調整する機能設けたいけどジェネリックの動作調べるの面倒。
    public static class HarmonyPatch_FacialAnimation
    {

        private static readonly FieldInfo getPawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");

        internal static readonly MethodInfo DrawLocAndQuatMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new System.Type[]
        {//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool),
        });
        internal static IEnumerable<CodeInstruction> DrawFace_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(AccessTools.TypeByName("FacialAnimation.HarmonyPatches"), "targetHeadFacing"));
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHead));
                    HarmonyPatch_PawnRender.head = true;
                    continue;
                }
                yield return code;
            }
            if (!HarmonyPatch_PawnRender.head) Log.Warning($"[Stylish Rim] Failed adjust head transpiler.");
        }
        internal static void DrawGraphics_Prefix(Rot4 headFacing, Pawn ___pawn)
        {
            StylishAdjuster.rot4FA = headFacing;
            StylishAdjuster.pawnFA = ___pawn;
        }
        internal static void DrawGraphics_Postfix()
        {
            StylishAdjuster.pawnFA = null;
        }
        internal static void DrawMeshNowOrLaterWithScale_Prefix(ref float scaleW, ref float scaleH)
        {
            StylishAdjuster.FacialAnimationAddScale(ref scaleW, ref scaleH);
        }
        /*
        internal static IEnumerable<CodeInstruction> DrawBodyPart_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // なんか二回サイズ調整の処理行わないといけない
            // 片方だけでよかったりしねえかなぁ
            // よかった
            MethodInfo drawMethod = (from m in AccessTools.GetDeclaredMethods(AccessTools.TypeByName("FacialAnimation.GraphicHelper")) where m.GetParameters().Count() == 6 select m).FirstOrDefault();
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(drawMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp"), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishAdjuster), nameof(StylishAdjuster.FacialAnimationAddScale)));
                    continue;
                }
                yield return code;
            }
        }
        */
    }
}
