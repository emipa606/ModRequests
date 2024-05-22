using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace TakeOffIndoor
{
    public static class Harmony_JetPack_ResolveApparelGraphics
    {
        public static void PatchJetPack(Harmony harmony)
        {
            //Tranpiler
            MethodInfo original = AccessTools.Method(ModUtil.JetPack_RAG_JPPP, "PostFix");
            HarmonyMethod transpiler = Util.HM(typeof(Harmony_JetPack_ResolveApparelGraphics), "Transpiler");
            transpiler.priority = int.MinValue;
            harmony.Patch(original, null, null, transpiler);
            Log.Message("TakeOffCoat:JetPack Patched.");
            debug.WriteLine("JetPack Patched.");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo m_wornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = codes.FindIndex(c => c.opcode == OpCodes.Callvirt && c.operand != null && (MethodInfo)c.operand == m_wornApparel);
            //codes[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Harmony_WornApparel), "GetWornApparelCashed"));
            if (ModUtil.ExistAppearanceClothes)
            {
                codes[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(ModUtil.AppearanceClothes_RGP, "GetWornApparel"));
            }
            else
            {
                codes[pos] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Harmony_WornApparel), "GetWornApparel"));
            }

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }

    }
}
