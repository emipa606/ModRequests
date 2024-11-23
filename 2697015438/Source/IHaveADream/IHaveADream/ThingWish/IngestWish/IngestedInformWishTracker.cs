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

    [HarmonyPatch(typeof(Thing), "Ingested")]
    public static class IngestedInformWishTracker
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].operand.ToString().Contains("IngestedCalculateAmounts"))
                    {
                        MethodInfo m_MyExtraMethod = SymbolExtensions.GetMethodInfo(() => InformWish(null, null, 0, 0));
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldloc, 3);
                        yield return new CodeInstruction(OpCodes.Ldloc, 4);
                        yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                        
                        for (int j = i + 1; j < codes.Count; j++) yield return codes[j];
                        break;
                    }
                }
            }

        }
        static void InformWish(Pawn pawn, Thing thing, int amount, float nutriment)
        {
            Pawn_WishTracker wishes = pawn.wishes();
            if (wishes == null) return;
            wishes.TryResolveIngestible(thing, amount, nutriment);
        }
    }
}
