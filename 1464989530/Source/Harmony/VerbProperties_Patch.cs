#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) })]
    [UsedImplicitly]
    public static class VerbProperties_AdjustedCooldown

    {
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler(
                        IEnumerable<CodeInstruction> instructions
                    )
        {
            var instructionsList = instructions.ToList();
            var signifyingMethod = AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue));

            for (int i = 0; i < instructionsList.Count; i++)
            {

                var current = instructionsList[i];

                yield return current;
#if HARM12
                if (current.opcode == OpCodes.Call && current.operand == signifyingMethod)
#else
                if (/*current.opcode == OpCodes.Call && current.operand == signifyingMethod*/current.Is(OpCodes.Call, signifyingMethod))
#endif
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatHelpers), nameof(CombatHelpers.AdjustCooldownForGlow)));
                }
            }
        }
    }
}
