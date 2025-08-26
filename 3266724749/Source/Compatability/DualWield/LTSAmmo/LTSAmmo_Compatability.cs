using System.Collections.Generic;
using System.Configuration;
using System.Reflection.Emit;
using System.Xml.Serialization;
using HarmonyLib;
using Verse;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl.Compatability.DualWield.LTSAmmo;

public class LTSAmmo_Compatability
{
    // STFU im using a patch on my own code for this, its shit but im lazy to add dependencies to this
    [HarmonyPatch(typeof(LTSAmmo_Compatability), nameof(LTSAmmo_Compatability.IsAvailable))]
    private static class LoadPatchCompatability
    {
        static bool Prepare()
        {
            return Settings.dualWieldEnabled && Settings.usingLTSAmmo;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var local = generator.DeclareLocal(AccessTools.TypeByName("Ammunition.Components.KitComponent"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldloca_S, local);
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(AccessTools.TypeByName("Ammunition.Logic.AmmoLogic"), "AmmoCheck"));
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
    public static bool IsAvailable(Pawn caster, Thing instance)
    {
        return true;
    }
    
}
