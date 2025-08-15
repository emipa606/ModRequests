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
    public static class HarmonyPatch_PawnRender
    {
        public static MethodInfo RenderPawnInternalMethod = AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal");
        private static readonly FieldInfo getPawnField = AccessTools.Field(typeof(PawnRenderer), "pawn");

        internal static readonly MethodInfo DrawMatrixMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new System.Type[]
        {//Mesh mesh, Matrix4x4 matrix, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Matrix4x4),
                typeof(Material),
                typeof(bool),
        });
        internal static readonly MethodInfo DrawLocAndQuatMethod = AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater), new System.Type[]
        {//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(bool),
        });


        [HarmonyPriority(401)]
        internal static void GetBlitMeshUpdatedFrame_Prefix(Pawn ___pawn, PawnTextureAtlasFrameSet frameSet, PawnDrawMode drawMode)
        {
            if (!StylishTextureAtlas.IgnoreHeadOnly) return;
            if (!StylishAtlasManager.DrawModeIs(___pawn.ThingID, drawMode))
            {
                for (int i = 0; i < frameSet.isDirty.Length; i++)
                {
                    frameSet.isDirty[i] = true;
                }
            }
        }
        [HarmonyPriority(401)]
        internal static IEnumerable<CodeInstruction> GetBlitMeshUpdatedFrame_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int step = 0;
            foreach (CodeInstruction code in instructions)
            {
                if (step == 0 && code.opcode == OpCodes.Stloc_0)
                {
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.IgnoreHeadIndex));
                    step++;
                }
                yield return code;
            }
        }
        internal static void RenderPawnAt_Prefix(Pawn ___pawn, ref Vector3 drawLoc)
        {
            // 身体のサイズに合わせてポーンをずらすやつ
            StylishAdjuster.AddPawnOffsetBodyDiff(ref drawLoc, ___pawn);
        }
        internal static IEnumerable<CodeInstruction> RenderPawnAt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 身体のサイズに合わせてポーンをずらすやつ
            MethodInfo zoomRootGetter = AccessTools.PropertyGetter(typeof(CameraDriver), nameof(CameraDriver.ZoomRootSize));
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(zoomRootGetter))
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishAtlasManager), nameof(StylishAtlasManager.DisableZoomCacheOff));
                    continue;
                }
                yield return code;
            }
        }
        [HarmonyPriority(401)]
        internal static void DrawEquipment_Prefix(Pawn ___pawn, Rot4 pawnRotation)
        {
            StylishDrawer.DynamicPartStloc(___pawn, pawnRotation);
        }
        internal static void DrawEquipment_Postfix()
        {
            StylishDrawer.DynamicPartPostfix();
        }
        [HarmonyPriority(401)]
        internal static IEnumerable<CodeInstruction> DrawEquipmentAiming_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int step = 0;
            /*
            MethodInfo DrawMesh = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new System.Type[]
            {// Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer
                typeof(Mesh),
                typeof(Matrix4x4),
                typeof(Material),
                typeof(int)
            });
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawMesh))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart));
                    result = true;
                    continue;
                }
                yield return code;
            }
            */
            MethodInfo Matrix4x4TRS = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.TRS));
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(Matrix4x4TRS))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart1_4));
                    step++;
                    continue;
                }
                else if (step == 1 && code.opcode == OpCodes.Ldloc_0)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustDynamicPart1_4FlipMesh));
                    step++;
                    continue;
                }
                yield return code;
            }
            if (step < 2) Log.Warning($"[Stylish Rim] Failed DrawEquipmentAiming_Transpiler.");
        }

        [HarmonyAfter("OskarPotocki.VFECore")]
        internal static IEnumerable<CodeInstruction> RenderPawnInternal_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 頭の位置調整やAlien無しの頭メッシュ取得について
            MethodInfo BaseHeadOffsetAtMethod = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt));
            FieldInfo humanlikeHeadMeshField = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.humanlikeHeadSet));
            FieldInfo headGraphicField = AccessTools.Field(typeof(PawnGraphicSet), nameof(PawnGraphicSet.headGraphic));

            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(BaseHeadOffsetAtMethod))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return CodeInstruction.Call(typeof(PawnRenderFlagsExtension), nameof(PawnRenderFlagsExtension.FlagSet));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.AdjustHeadOffset));
                    continue;
                }
                else if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHead));
                    head = true;
                    continue;
                }
                yield return code;
            }
            if (!StylishModSettings.includeFacialAnimation && !head) Log.Warning($"[Stylish Rim] Failed adjust head transpiler.");
        }
        internal static void DrawPawnBody_Prefix()
        {
            StylishDrawer.isDrawPawnBody = true;
        }
        internal static void DrawPawnBody_Postfix()
        {
            StylishDrawer.isDrawPawnBody = false;
        }
        [HarmonyAfter("OskarPotocki.VFECore")]
        internal static IEnumerable<CodeInstruction> DrawPawnBody_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int step = 0;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    if (step == 0)
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparel)));
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustBodyTatoo)));
                    }
                    step++;
                    continue;
                }
                yield return code;
            }
            if (step == 0) Log.Warning($"[Stylish Rim] Failed adjust apparel transpiler.");
        }
        [HarmonyAfter("OskarPotocki.VFECore")]
        internal static IEnumerable<CodeInstruction> DrawBodyGenes_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//Vector3 rootLoc, Quaternion quat, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustBodyGenes)));
                    continue;
                }
                yield return code;
            }
        }
        [HarmonyAfter("OskarPotocki.VFECore")]
        internal static IEnumerable<CodeInstruction> DrawPawnFur_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//Vector3 rootLoc, Quaternion quat, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustPawnFur)));
                    continue;
                }
                yield return code;
            }
        }
        [HarmonyAfter("OskarPotocki.VFECore")]
        internal static IEnumerable<CodeInstruction> DrawBodyApparel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 実験段階のベルト装備の調整システム
            // エポナさんバックパックデカすぎ問題への対処だったり
            // 処理が暫定的すぎるので何かしら問題おきるのでは。

            FieldInfo bodyTypeField = AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.bodyType));
            MethodInfo beltScaleMethod = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltScaleAt));
            MethodInfo beltOffsetMethod = AccessTools.Method(typeof(WornGraphicData), nameof(WornGraphicData.BeltOffsetAt));
            CodeInstruction beltMatrix = new CodeInstruction(OpCodes.Ldloc_S, 9);
            CodeInstruction ldlocMaterialCode = new CodeInstruction(OpCodes.Ldloc_S, 4);
            bool apparelShell = false;
            bool apparelUtility = false;
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustApparelShell));
                    apparelShell = true;
                    continue;
                }
/*uooooooooooooooooooo
                else if (code.OperandIs(bodyTypeField))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeBodyTypeByPawn));
                    continue;
                }
*/
                else if (code.OperandIs(DrawMatrixMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustBelt));
                    apparelUtility = true;
                    continue;
                }
                else if (code.OperandIs(beltOffsetMethod))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustBeltOffset));
                    continue;
                }
                yield return code;
            }
            if (!apparelShell) Log.Warning($"[Stylish Rim] Failed adjust apparel shell transpiler.");
            if (!apparelUtility) Log.Warning($"[Stylish Rim] Failed adjust apparel utility transpiler.");
        }
        public static bool head = false;
        [HarmonyPriority(401)]
        internal static IEnumerable<CodeInstruction> DrawHeadHair_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool beard = false;
            bool hair = false;
            int step = 0;
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg, 5);
                    yield return new CodeInstruction(OpCodes.Ldarg, 7);
                    if (step == 0)
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustFaceTatoo));
                        step++;
                    }
                    else if (step == 1)
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustBeard));
                        step++;
                        beard = true;
                    }
                    else
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHair));
                        hair = true;
                    }
                    continue;
                }
                yield return code;
            }
            if (!StylishRim.showHair && !hair) Log.Warning($"[Stylish Rim] Failed adjust hair transpiler.");
            if (!beard) Log.Warning($"[Stylish Rim] Failed adjust beard transpiler.");
        }
        [HarmonyBefore("Quickfast")]
        internal static void DrawHeadHair_Prefix(Pawn ___pawn)
        {
            StylishDrawer.pawn = ___pawn;
        }
        internal static void RenderPawnInternal_Postfix()
        {
            StylishDrawer.pawn = null;
            StylishDrawer.spss = null;
        }
        public static System.Type nestedType;
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
                if (code.OperandIs(DrawMatrixMethod))
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
                if (code.OperandIs(DrawLocAndQuatMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(nestedType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustHeadGenes));
                    continue;
                }
                yield return code;
            }
        }
    }
}
