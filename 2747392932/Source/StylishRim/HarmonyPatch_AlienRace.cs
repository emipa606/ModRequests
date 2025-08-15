using System.Linq;
using AlienRace;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace StylishRim
{
    // AlienRaceの処理に寄生するためのパッチ集。
    public static class HarmonyPatch_AlienRace
    {
        /*
        internal static IEnumerable<CodeInstruction> GetPawnMesh_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // 各種メッシュのサイズ調整する根幹部分
            // だったけど今は身体のメッシュしか取得しておらんのだ……

            MethodInfo getMeshMethod = AccessTools.Method(typeof(GraphicMeshSet), nameof(GraphicMeshSet.MeshAt));
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 1 && codes[i + 1].OperandIs(getMeshMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AdjustMeshSet));
                }
                yield return codes[i];
            }
        }
        */
        public static Pawn p;
        public static PawnRenderFlags flags;
        public static Rot4 rot;
        public static PawnStyleSet pss;
        public static Dictionary<int, StylishAddon> sa;
        public static int addonIndex = 99;
        public static AlienPartGenerator.BodyAddon ba;

        internal static void DrawAddons_Prefix(Pawn pawn, PawnRenderFlags renderFlags, Rot4 rotation)
        {
            p = pawn;
            flags = renderFlags;
            rot = rotation;
            pss = PawnStyleSet.Get(pawn);
            if (pss != null) sa = pss.Calc.GetAddons(pawn.story.bodyType.defName);
        }
        internal static void DrawAddons_Postfix()
        {
            p = null;
            pss = null;
            sa = null;
            addonIndex = 99;
            ba = null;
        }
        internal static void SetAddonAndIndex(int i, AlienPartGenerator.BodyAddon bodyAddon)
        {
            addonIndex = i;
            ba = bodyAddon;
        }
        internal static void SetAddon(AlienPartGenerator.BodyAddon bodyAddon)
        {
            ba = bodyAddon;
        }
        internal static IEnumerable<CodeInstruction> DrawAddons_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation
            // アドオンの各種補正をかける処理
            string stLocS7OperandString = "UnityEngine.Vector3 (7)";
            bool completeOffset = false;
            if (instructions.ToList().Any(x => x.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod)))
            {
                foreach (CodeInstruction code in instructions)
                {
                    if (!completeOffset && OpCodes.Stloc_S == code.opcode && stLocS7OperandString == code.operand.ToString())
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(HarmonyPatch_AlienRace.SetAddonAndIndex));
                        yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AddAddonOffset));
                        completeOffset = true;
                    }
                    else if (code.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod))
                    {
                        yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustAddons));
                        continue;
                    }
                    /*
                    else if (code.opcode == OpCodes.Stloc_3)
                    {
                        yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(NoPortrait));
                    }
                    */
                    yield return code;
                }
            }
            else
            {
                int step = 0;
                MethodInfo nestedMethod = AccessTools.GetDeclaredMethods(typeof(HarmonyPatches)).Where(x => x.Name.Contains(StylishRim.NESTED_CLASS_DRAW_ADDON)).FirstOrDefault();
                foreach (CodeInstruction code in instructions)
                {
                    if (code.OperandIs(nestedMethod))
                    {
                        if (step == 0)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                            yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(HarmonyPatch_AlienRace.SetAddonAndIndex));
                        }
                        else
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 10);
                            yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(HarmonyPatch_AlienRace.SetAddon));
                        }
                        yield return code;
                        step++;
                        continue;
                    }
                    yield return code;
                }
            }

        }
        internal static IEnumerable<CodeInstruction> DrawAddonCompiled_Transpiler(IEnumerable<CodeInstruction> instructions)
        {//PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation
            // アドオンの各種補正をかける処理
            string stLocS0OperandString = "UnityEngine.Vector3 (0)";
            bool completeOffset = false;
            foreach (CodeInstruction code in instructions)
            {
                if (!completeOffset && OpCodes.Stloc_S == code.opcode && stLocS0OperandString == code.operand.ToString())
                {
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.AddAddonOffset));
                }
                else if (code.OperandIs(HarmonyPatch_PawnRender.DrawLocAndQuatMethod))
                {
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.DrawAdjustAddons));
                    continue;
                }
                /*
                else if (code.opcode == OpCodes.Stloc_3)
                {
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_AlienRace), nameof(NoPortrait));
                }
                */
                yield return code;
            }

        }
        /*
        public static void GetOffset_Prefix(ref bool portrait)
        {
            if (portrait && StylishModSettings.commonizationMesh) portrait = false;
        }
        public static bool NoPortrait(bool portrait)
        {
            if (StylishModSettings.commonizationMesh) return false;
            return portrait;
        }
        */

        internal static void CacheRenderPawnPrefix_Postfix(Pawn pawn, ref float cameraZoom, bool portrait)
        {
            // 描画範囲を最低限に抑えつつカメラのズーム調整で少しは軽くなるのでは？
            // 意味ないどころか重くなったらどうしよう。
            // とりあえず画質は良くなるっぽいので
            // BorderScale が同種族でも異なったら描画にバグ起こしやすいこれやばいどうしよ
            // BorderScale の調整は種族単位で行うことにした
            // 
            if (portrait) return;
            cameraZoom = StylishAdjuster.ZoomAdjuster(pawn, cameraZoom);
        }
        internal static IEnumerable<CodeInstruction> GetBorderSizeForPawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // borderScaleを上書きするのでGETしにいく
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ldsfld)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.GetPawnBorderScale));
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield break;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> GetAtlasSizeForPawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // atlasScaleを上書きするのでGETしにいく
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Ldsfld)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.GetPawnAtlasScale));
                    yield return new CodeInstruction(OpCodes.Ret);
                    yield break;
                }
                yield return code;
            }
        }
        internal static void ResolveAllGraphicsPrefix_PostFix()
        {
            StylishBodyChanger.Finish();
        }
        internal static IEnumerable<CodeInstruction> ResolveAllGraphicsPrefix_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo loadBodyPathField = AccessTools.Field(typeof(GraphicPaths), nameof(GraphicPaths.body));
            FieldInfo loadShaderTypeField = AccessTools.Field(typeof(GraphicPaths), nameof(GraphicPaths.skinShader));
            FieldInfo loadBodyTypeField = AccessTools.Field(typeof(RimWorld.Pawn_StoryTracker), nameof(RimWorld.Pawn_StoryTracker.bodyType));
            MethodInfo getSkinColorField1 = AccessTools.PropertyGetter(typeof(RimWorld.Pawn_StoryTracker), nameof(RimWorld.Pawn_StoryTracker.SkinColor));
            MethodInfo getSkinColorField2 = AccessTools.Method(typeof(AlienPartGenerator), nameof(AlienPartGenerator.SkinColor));
            FieldInfo hairHasStyleField = AccessTools.Field(typeof(StyleSettings), nameof(StyleSettings.hasStyle));
            bool prefix = false;
            int step = 0;
            foreach (CodeInstruction code in instructions)
            {
/*uoooooooooooooo
                if (!prefix && code.opcode == OpCodes.Stloc_0)
                {
                    prefix = true;
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.Init));
                    continue;
                }
                else if (code.opcode == OpCodes.Ldfld && code.OperandIs(loadBodyPathField))
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeBody));
                    continue;
                }
                else if (step < 2 && code.OperandIs(getSkinColorField1) || code.OperandIs(getSkinColorField2))
                {
                    step++;
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeColor));
                    continue;
                }
                else if (code.opcode == OpCodes.Ldfld && code.OperandIs(loadBodyTypeField))
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeBodyType));
                    continue;
                }
                else if (code.OperandIs(hairHasStyleField))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.ShowHairSetting));
                    continue;
                }
*/
                if (code.OperandIs(hairHasStyleField))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.Call(typeof(StylishAdjuster), nameof(StylishAdjuster.ShowHairSetting));
                    continue;
                }
                /*
                else if (step < 4 && code.opcode == OpCodes.Ldfld && code.OperandIs(loadShaderTypeField))
                {
                    step++;
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeShader));
                    continue;
                }
                */
                yield return code;
            }
        }

        //================================ PREFIX !!!!!!!!!!!!!!=================================//
        //
        // サイズバグを誘発してるメソッドを潰すのだ
        internal static bool TextureAtlasSameRace_Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        internal static IEnumerable<CodeInstruction> ResolveReferences_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo customPortraitDrawSizeField = AccessTools.Field(typeof(AlienPartGenerator), nameof(AlienPartGenerator.customPortraitHeadDrawSize));
            bool insertPrefix = false;
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Stfld && code.OperandIs(customPortraitDrawSizeField))
                {
                    insertPrefix = true;
                }
                else if (insertPrefix)
                {
                    yield return code;
                    yield return CodeInstruction.Call(typeof(StylishRedefinition), nameof(StylishRedefinition.RedifinePortraitDrawSize));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    insertPrefix = false;
                    continue;
                }
                yield return code;
            }
        }
    }
}
