using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
//using AppearanceClothes;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace TakeOffIndoor
{
    public static class Harmony_WornApparel_Trans
    {
        public static IEnumerable<CodeInstruction> RAG_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo m_wornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = codes.FindIndex(c => c.opcode == OpCodes.Callvirt && c.operand != null && (MethodInfo)c.operand == m_wornApparel);
            codes[pos] = new CodeInstruction(OpCodes.Callvirt, typeof(Harmony_WornApparel).GetMethod(nameof(Harmony_WornApparel.GetWornApparel)));

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }

        public static IEnumerable<CodeInstruction> TranspilerWornApparelAP(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //debug.WriteLine("twap");
            int pos;
            while((pos = codes.FindIndex(c => c.opcode == OpCodes.Call && c.operand != null && (MethodInfo)c.operand == typeof(Harmony_WornApparel).GetMethod(nameof(Harmony_WornApparel.GetWornApparel)))) >= 0)
            {
                codes[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetWornApparel"));
                //debug.WriteLine("wapd");
            }

            foreach (CodeInstruction code in codes)
            {
                //debug.WriteLine(code?.ToString()??"");
                yield return code;
            }
        }

        public static IEnumerable<CodeInstruction> APTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo m_wornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = codes.FindIndex(c => c.opcode == OpCodes.Callvirt && c.operand != null && (MethodInfo)c.operand == m_wornApparel);
            codes[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetWornApparel"));

            FieldInfo f_bodyType = AccessTools.Field(typeof(Pawn_StoryTracker), "bodyType");
            int pos2 = codes.FindIndex(c => c.opcode == OpCodes.Ldfld && c.operand != null && (FieldInfo)c.operand == f_bodyType);
            codes.Insert(pos2 + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetBodyTypeDef")));
            codes.RemoveRange(pos2 - 1, 2);

            pos2 = codes.FindIndex(c => c.opcode == OpCodes.Ldfld && c.operand != null && (FieldInfo)c.operand == f_bodyType);
            codes.Insert(pos2 + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetBodyTypeDef")));
            codes.RemoveRange(pos2 - 1, 2);

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }
        public static IEnumerable<CodeInstruction> BodyTypeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            FieldInfo f_bodyType = AccessTools.Field(typeof(Pawn_StoryTracker), "bodyType");
            int pos2 = codes.FindIndex(c => c.opcode == OpCodes.Ldfld && c.operand != null && (FieldInfo)c.operand == f_bodyType);
            codes.Insert(pos2 + 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(AppearanceClothes.PawnGraphicSet_ResolveApparelGraphics_Patch), "GetBodyTypeDef")));
            codes.RemoveRange(pos2 - 1, 2);

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }

        public static void PatchAppearanceClothes(Harmony harmony)
        {
            //AppearanceClothesの方にパッチを当てる
            MethodInfo original = AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetWornApparel");
            HarmonyMethod postfix = Util.HM(typeof(Harmony_WornApparel),nameof(Harmony_WornApparel.GetWornApparelPost));
            postfix.priority = int.MinValue;
            harmony.Patch(original, null, postfix);
            Log.Message("TakeOffCoat:AppearanceClothes Patched.");
            debug.WriteLine("AppearanceClothes Patched.");
        }

        public static void PatchResolveApparelGraphics(Harmony harmony)
        {
            //Tranpiler
            MethodInfo original = AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics");
            HarmonyMethod transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "RAG_Transpiler");
            harmony.Patch(original, null, null, transpiler);
            debug.WriteLine("TakeOffCoat:ResolveApparelGraphics Patched.");
        }

        public static void PatchWornApparelCachedAP(Harmony harmony)
        {
            //Tranpiler
            MethodInfo original = typeof(Harmony_WornApparel).GetMethod(nameof(Harmony_WornApparel.GetWornApparelCached));
            HarmonyMethod transpiler = Util.HM(typeof(Harmony_WornApparel_Trans), "TranspilerWornApparelAP");
            harmony.Patch(original, null, null, transpiler);
            debug.WriteLine("TakeOffCoat:GetWornApparelCashed Patched.");
        }

    }
}
