using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements.Experimental;
using HugsLib.Shell;
using System.Reflection.Emit;

namespace USESetApparelDrawOrder
{
    [HarmonyPatch]
    public static class Patch_DrawHeadHair_DrawApparel_Transpiler
    {
        private static Type displayType;

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            MethodInfo methodInfo = typeof(PawnRenderer).GetNestedTypes(AccessTools.all).SelectMany((Type type) => type.GetMethods(AccessTools.all)).FirstOrDefault((MethodInfo x) => x.Name.Contains("<DrawHeadHair>") && x.Name.Contains("DrawApparel"));
            displayType = methodInfo.DeclaringType;
            return methodInfo;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> codes = codeInstructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;
            AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
            {
            typeof(Mesh),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Material),
            typeof(bool)
            });
            CodeInstruction getThis = codes.First((CodeInstruction ins) => ins.opcode == OpCodes.Ldfld);
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!foundFirstBlock && codes[i].opcode == OpCodes.Stloc_0)
                {
                    foundFirstBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return getThis.Clone();
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloca, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), "TryModifyMeshRef"));
                }
                if (foundFirstBlock && !foundSecondBlock && codes[i].opcode == OpCodes.Stloc_1 && codes[i + 1].opcode == OpCodes.Ldloc_0)
                {
                    foundSecondBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(displayType, "onHeadLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), "TryModifyHeadGearLocRef"));
                }
                if (foundSecondBlock && !foundThirdBlock && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    foundThirdBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(displayType, "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair_DrawApparel_Transpiler), "TryModifyHeadGearLoc"));
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }
            }
            if (!foundFirstBlock || !foundSecondBlock || !foundThirdBlock)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawHeadHair+DrawApparel failed.");
            }
        }

        public static Vector3 TryModifyHeadGearLoc(Rot4 rot, Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            //ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //if (modExtension != null)
            //{
            //    if (modExtension.apparelDrawSettings != null)
            //    {
            //        return modExtension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //    if (modExtension.headgearDrawSettings != null)
            //    {
            //        return modExtension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //}

            loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;

            return loc;
        }

        public static void TryModifyHeadGearLocRef(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            //ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //if (modExtension != null)
            //{
            //    if (modExtension.apparelDrawSettings != null)
            //    {
            //        loc = modExtension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //    else if (modExtension.headgearDrawSettings != null)
            //    {
            //        loc = modExtension.headgearDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //}

            loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
        }

        public static void TryModifyMeshRef(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            //    ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //    if (modExtension != null)
            //    {
            //        if (modExtension.apparelDrawSettings != null)
            //        {
            //            mesh = modExtension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //        else if (modExtension.headgearDrawSettings != null)
            //        {
            //            mesh = modExtension.headgearDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //    }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
    public static class Patch_PawnRenderer_DrawPawnBody_Transpiler
    {
        public static Vector3 oldVector;

        public static Mesh oldMesh;

        public static Dictionary<Material, ApparelGraphicRecord> mappedValues = new Dictionary<Material, ApparelGraphicRecord>();

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> codes = codeInstructions.ToList();
            bool found1 = false;
            bool found2 = false;
            MethodInfo drawMeshNowOrLaterMethod = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
            {
            typeof(Mesh),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Material),
            typeof(bool)
            });
            for (int i = 0; i < codes.Count; i++)
            {
                if (!found1 && i > 3 && codes[i - 1].opcode == OpCodes.Stloc_S && codes[i - 1].operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 5)
                {
                    found1 = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), "ModifyApparelLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), "ModifyMesh"));
                }
                yield return codes[i];
                if (found1 && !found2 && codes[i].Calls(drawMeshNowOrLaterMethod))
                {
                    found2 = true;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), "ResetVector"));
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_PawnRenderer_DrawPawnBody_Transpiler), "ResetMesh"));
                }
            }
            if (!found1 || !found2)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawPawnBody failed.");
            }
        }

        public static void ModifyApparelLoc(Rot4 rot, ref Vector3 vector, Material mat)
        {
            oldVector = vector;
            if (mat != null && mappedValues.TryGetValue(mat, out var value))
            {
                //ApparelDrawPosExtension modExtension = value.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
                //if (modExtension?.apparelDrawSettings != null)
                //{
                //    vector = modExtension.apparelDrawSettings.GetDrawPosOffset(rot, vector);
                //}

                vector.y += value.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
            }
        }

        public static void ResetVector(ref Vector3 vector)
        {
            //    vector = oldVector;
        }

        public static void ModifyMesh(Pawn pawn, ref Mesh mesh, Material mat)
        {
            //    oldMesh = mesh;
            //    if (mat != null && mappedValues.TryGetValue(mat, out var value))
            //    {
            //        ApparelDrawPosExtension modExtension = value.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //        if (modExtension?.apparelDrawSettings != null)
            //        {
            //            mesh = modExtension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //    }
        }

        public static void ResetMesh(ref Mesh mesh)
        {
            //    mesh = oldMesh;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class Harmony_PawnRenderer_DrawBodyApparel
    {
        public static Vector3 oldVector;

        public static Mesh oldMesh;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo drawMeshNowOrLaterMethodMatrix = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[4]
            {
            typeof(Mesh),
            typeof(Matrix4x4),
            typeof(Material),
            typeof(bool)
            });
            MethodInfo drawMeshNowOrLaterMethodVector3 = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[5]
            {
            typeof(Mesh),
            typeof(Vector3),
            typeof(Quaternion),
            typeof(Material),
            typeof(bool)
            });
            MethodInfo translateMethod = AccessTools.Method(typeof(Matrix4x4), "Translate");
            List<CodeInstruction> codes = instructions.ToList();
            bool foundFirstBlock = false;
            bool foundSecondBlock = false;
            bool foundThirdBlock = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!foundFirstBlock && i > 3 && codes[i - 3].opcode == OpCodes.Ldc_R4 && codes[i - 3].OperandIs(0.00289575267f) && codes[i - 2].opcode == OpCodes.Add && codes[i - 1].opcode == OpCodes.Stind_R4)
                {
                    foundFirstBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ModifyShellLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 3);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ModifyShellMesh"));
                }
                yield return codes[i];
                if (foundFirstBlock && !foundSecondBlock && codes[i].Calls(drawMeshNowOrLaterMethodVector3))
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ResetLoc"));
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ResetMesh"));
                }
                if (!foundSecondBlock && codes[i + 2].Calls(translateMethod) && codes[i + 3].opcode == OpCodes.Ldloc_1)
                {
                    foundSecondBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ModifyPackLoc"));
                }
                if (foundSecondBlock && !foundThirdBlock && codes[i].Calls(drawMeshNowOrLaterMethodMatrix))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ResetLoc"));
                }
                if (!foundThirdBlock && i > 3 && codes[i - 2].Calls(drawMeshNowOrLaterMethodMatrix) && codes[i - 1].opcode == OpCodes.Br_S)
                {
                    foundThirdBlock = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ModifyPackLoc"));
                }
                if (foundThirdBlock && codes[i].Calls(drawMeshNowOrLaterMethodVector3))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_PawnRenderer_DrawBodyApparel), "ResetLoc"));
                }
            }
            if (!foundFirstBlock || !foundSecondBlock || !foundThirdBlock)
            {
                Log.Error("[Vanilla Framework Expanded] Transpiler on PawnRenderer:DrawBodyApparel failed.");
            }
        }

        public static void ModifyPackLoc(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            oldVector = loc;
            //ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //if (modExtension != null)
            //{
            //    if (modExtension.apparelDrawSettings != null)
            //    {
            //        loc = modExtension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //    else if (modExtension.packPosDrawSettings != null)
            //    {
            //        loc = modExtension.packPosDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //}
            loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
        }

        public static void ModifyShellLoc(Rot4 rot, ref Vector3 loc, ApparelGraphicRecord apparelRecord)
        {
            oldVector = loc;
            //ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //if (modExtension != null)
            //{
            //    if (modExtension.apparelDrawSettings != null)
            //    {
            //        loc = modExtension.apparelDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //    else if (modExtension?.shellPosDrawSettings != null)
            //    {
            //        loc = modExtension.shellPosDrawSettings.GetDrawPosOffset(rot, loc);
            //    }
            //}
            loc.y += apparelRecord.sourceApparel.TryGetComp<USEDrawOrderSetComp>().drawOrderEntered / 2500f;
        }

        public static void ResetLoc(ref Vector3 loc)
        {
            //    loc = oldVector;
        }

        public static void ModifyShellMesh(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            //    oldMesh = mesh;
            //    ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //    if (modExtension != null)
            //    {
            //        if (modExtension.apparelDrawSettings != null)
            //        {
            //            mesh = modExtension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //        else if (modExtension.shellPosDrawSettings != null)
            //        {
            //            mesh = modExtension.shellPosDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //    }
        }

        public static void ModifyPackMesh(Pawn pawn, ref Mesh mesh, ApparelGraphicRecord apparelRecord)
        {
            //    oldMesh = mesh;
            //    ApparelDrawPosExtension modExtension = apparelRecord.sourceApparel.def.GetModExtension<ApparelDrawPosExtension>();
            //    if (modExtension != null)
            //    {
            //        if (modExtension.apparelDrawSettings != null)
            //        {
            //            mesh = modExtension.apparelDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //        else if (modExtension.packPosDrawSettings != null)
            //        {
            //            mesh = modExtension.packPosDrawSettings.TryGetNewMesh(mesh, pawn);
            //        }
            //    }
        }

        public static void ResetMesh(ref Mesh mesh)
        {
            //    mesh = oldMesh;
        }
    }
}
