using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace UINotIncluded.Patches
{
    [HarmonyPatch(typeof(InspectPaneUtility), "DoTabs")]
    internal class InspectPaneUtility_Patch
    {
        private static readonly float VANILLA_BAR_HEIGHT = 35f;

        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var getAdjustedMethod = AccessTools.Method(typeof(InspectPaneUtility_Patch), nameof(getAdjustedDifference));

            var injected = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!injected && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 30f)
                {
                    injected = true;

                    yield return codes[i]; // yield return the original 30f
                    i++;
                    yield return codes[i]; // yield return the sub instruction
                    i++;

                    yield return new CodeInstruction(OpCodes.Call, getAdjustedMethod);
                    yield return new CodeInstruction(OpCodes.Add);
                }

                yield return codes[i];
            }

        }

        private static float getAdjustedDifference()
        {
            return VANILLA_BAR_HEIGHT - Settings.barsHeight;
        }
    }
}
