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
//using AlienRace.ExtendedGraphics; usingせずにobjectのままPropertyInfoだけで扱う



namespace TakeOffIndoor
{
    public class Harmony_AlienRace_ExtendedGraphics
    {
        public static Pawn GetPawn(object obj) //obj:ExtendedGraphicsPawnWrapper
        {
            return (Pawn)ModUtil.AlienRace_EGPW_WrappedPawn.GetValue(obj);
        }
        public static bool Prefix(ref IEnumerable<ApparelProperties> __result,ref object __instance) //instance:ExtendedGraphicsPawnWrapper
        {
            Pawn pawn = GetPawn(__instance);
            __result = Harmony_WornApparel.GetWornApparelCached(pawn.apparel).Select(ap => ap.def.apparel) ?? Enumerable.Empty<ApparelProperties>();
            return false;
        }

        //virtualだしpostにした方が良いかも知れない
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo m_wornApparel = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel");
            int pos = codes.FindIndex(c =>c.opcode==OpCodes.Call && c.operand != null && (MethodInfo)c.operand == m_wornApparel);

            if (pos > 0)
            {
                debug.WriteLine(codes[pos].opcode.ToString() + ":" + codes[pos].operand.ToString());
                Log.Message("TakeOffCoat:WornApparel Transpiler.");
                codes[pos] = new CodeInstruction(codes[pos].opcode, typeof(Harmony_WornApparel).GetMethod(nameof(Harmony_WornApparel.GetWornApparelCached)));
                Log.Message("success.");
            }
            else
            {
                Log.Warning("TakeOffCoat:Patch HAR Failed, Please report.");
            }

            foreach (CodeInstruction code in codes)
            {
                yield return code;
            }
        }
        public static void PatchAlienRace(Harmony harmony)
        {
            if (!TakeOffSettings.patchCanDrawAddon) return;
            MethodInfo original = AccessTools.Method(ModUtil.AlienRace_EGPW, "GetWornApparel");
            //HarmonyMethod transpiler = Util.HM(typeof(Harmony_AlienRace_ExtendedGraphics), "Transpiler");
            HarmonyMethod prefix = Util.HM(typeof(Harmony_AlienRace_ExtendedGraphics), "Prefix");
            harmony.Patch(original, prefix, null, null);
            Log.Message("TakeOffCoat:HAR Patched.");
            debug.WriteLine("HAR Patched.");

            if (ModUtil.ExistAppearanceClothes)
            {
                Harmony_WornApparel_Trans.PatchWornApparelCachedAP(harmony);
            }
        }
    }
}
