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
{    public static class Harmony_AlienRace_CanDrawAddon
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo m_wornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = codes.FindIndex(c => c.opcode == OpCodes.Callvirt && c.operand != null && (MethodInfo)c.operand == m_wornApparel);

            codes[pos] = new CodeInstruction(OpCodes.Callvirt, typeof(Harmony_WornApparel).GetMethod(nameof(Harmony_WornApparel.GetWornApparelCached)));

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }

        public static void PatchAlienRace(Harmony harmony)
        {
            if (!TakeOffSettings.patchCanDrawAddon) return;
            MethodInfo original = AccessTools.Method(ModUtil.AlienRace_APG, "CanDrawAddon",new Type[]{ typeof(Pawn)});
            HarmonyMethod transpiler = Util.HM(typeof(Harmony_AlienRace_CanDrawAddon), "Transpiler");
            harmony.Patch(original, null, null, transpiler);
            Log.Message("TakeOffCoat:HAR Patched.");
            debug.WriteLine("HAR Patched.");

            if (ModUtil.ExistAppearanceClothes)
            {
                Harmony_WornApparel_Trans.PatchWornApparelCachedAP(harmony);
            }
        }
    }
}
