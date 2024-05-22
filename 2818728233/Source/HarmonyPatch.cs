using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace growzones {
    [StaticConstructorOnStartup]
    internal static class HarmonyInit{
        static HarmonyInit(){
            Harmony harmony = new Harmony("whatamidoing.growZonesAnywhere");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Designator_ZoneAdd_Growing), "CanDesignateCell")]
    public static class Designator_ZoneAdd_Growing_CanDesignateCell_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            var jumpIndex = -1;
            var startIndex = -1;
            var endIndex = -1;
            Label label = il.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brtrue_S)
                {
                    jumpIndex = i;
                    break;
                }        
            }
            for(var j = jumpIndex + 1; j < codes.Count; j++)
            {
                if (codes[j].opcode == OpCodes.Ret)
                {
                    startIndex = j;
                    break;
                }
            }
            for(var k = startIndex + 1; k < codes.Count; k++)
            {
                if (codes[k].opcode == OpCodes.Ldc_I4_1 && codes[k - 1].opcode == OpCodes.Ret)
                {
                    endIndex = k;
                    break;
                }
            }
            codes[endIndex].labels.Add(label);
            codes[jumpIndex].operand = label;
            codes.RemoveRange(startIndex + 1, (endIndex - startIndex) - 1);

            return codes.AsEnumerable();
        }
    }
}
