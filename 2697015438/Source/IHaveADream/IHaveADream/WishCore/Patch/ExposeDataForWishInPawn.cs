using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;

namespace HDream
{

    [HarmonyPatch(typeof(Pawn), "ExposeData")]
    public static class ExposeDataForWishInPawn
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            bool mindstatefound = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Ldstr && (codes[i].operand as string) == "mindState") mindstatefound = true;
                if(mindstatefound && codes[i].opcode == OpCodes.Call)
                {

                    MethodInfo m_MyExtraMethod = SymbolExtensions.GetMethodInfo(() => ExposeData(null));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                    for (int j = i + 1; j < codes.Count; j++) yield return codes[j];
                    break;
                }
            }
        }
        static void ExposeData(Pawn pawn)
        {
            Pawn_WishTracker wishes = pawn.wishes();
            bool shouldAddTracker = (wishes == null) ? true :  false;
            Scribe_Deep.Look(ref wishes, "wishes", pawn);
            if (wishes != null && shouldAddTracker) pawn.CreatePawn_WishTracker(wishes);
        }
    }
}
