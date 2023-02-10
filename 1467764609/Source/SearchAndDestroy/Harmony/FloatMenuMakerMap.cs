using Harmony;
using RimWorld;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace SearchAndDestroy.Harmony
{
    /*
    [HarmonyPatch]
    public class FloatMenuMakerMap_GotoLocationOption
    {
        static MethodBase TargetMethod()//The target method is found using the custom logic defined here
        {
            var predicateClass = typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all)
                .FirstOrDefault(t => t.FullName.Contains("c__AnonStorey1B"));//c__Iterator2 is the hidden object's name, the number at the end of the name may vary. View the IL code to find out the name
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("m__0")); //Look for the method MoveNext inside the hidden iterator object
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, GetHiddenPawn());
            yield return new CodeInstruction(OpCodes.Call, typeof(FloatMenuMakerMap_GotoLocationOption).GetMethod("CustomPrefix"));

            var instructionsList = new List<CodeInstruction>(instructions);
            foreach(CodeInstruction instruction in instructionsList)
            {
                yield return instruction;
            }

        }

        public static void CustomPrefix(Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (pawnData.SD_enabled)
            {
                pawnData.SD_enabled = false;
            }

        }

        private static FieldInfo GetHiddenPawn()
        {
            var predicateClass = typeof(FloatMenuMakerMap).GetNestedTypes(AccessTools.all)
    .FirstOrDefault(t => t.FullName.Contains("c__AnonStorey1B"));
            return predicateClass.GetFields(AccessTools.all).FirstOrDefault(m => m.Name.Contains("pawn"));
        }

    }
    */
}