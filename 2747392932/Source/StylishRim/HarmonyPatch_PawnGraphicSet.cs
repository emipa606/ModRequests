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
    public static class HarmonyPatch_PawnGraphicSet
    {
        private static readonly FieldInfo getPawnField = AccessTools.Field(typeof(PawnGraphicSet), "pawn");
        internal static void MatsBodyBaseAt_Prefix(Pawn ___pawn, ref int ___cachedMatsBodyBaseHash)
        {
            if (!StylishDrawer.isDrawPawnBody) return;
            PawnStyleSet pss = PawnStyleSet.Get(___pawn);
            if (pss != null)
            {
                if (pss.clearCaches.Contains(___pawn.ThingID))
                {
                    ___cachedMatsBodyBaseHash = -1;
                    pss.clearCaches.Remove(___pawn.ThingID);
                }
            }
        }
        internal static IEnumerable<CodeInstruction> MatsBodyBaseAt_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int step = 0;
            CodeInstruction eyeCoverField = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ApparelLayerDefOf), nameof(ApparelLayerDefOf.EyeCover)));
            MethodInfo matAtMethod = AccessTools.Method(typeof(Graphic), nameof(Graphic.MatAt));
            foreach (CodeInstruction code in instructions)
            {
                if (code.opcode == OpCodes.Stfld)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.ApparelAdjusterInit));
                    continue;

                }
                else if (step == 0 && code.OperandIs(eyeCoverField.operand))
                {
                    step++;
                }
                else if (step == 1 && code.opcode == OpCodes.Callvirt)
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return CodeInstruction.Call(typeof(StylishDrawer), nameof(StylishDrawer.AddApparelAdjuster));
                    step++;
                    continue;
                }
                yield return code;
            }
        }
        internal static IEnumerable<CodeInstruction> ResolveApparelGraphics_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo bodyTypeField = AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.bodyType));
            bool body = false;
            foreach (CodeInstruction code in instructions)
            {
/*uoooooooo
                if (code.OperandIs(bodyTypeField))
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, getPawnField);
                    yield return CodeInstruction.Call(typeof(StylishBodyChanger), nameof(StylishBodyChanger.ChangeBodyTypeByPawn));
                    body = true;
                    continue;
                }
*/
                yield return code;
            }
            //if (!body) Log.Warning($"[Stylish Rim] Failed change body apparel transpiler.");
        }
    }
}
