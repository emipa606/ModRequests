using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace GiveMeMeat
{
  [StaticConstructorOnStartup]
  public static class StartUp
  {
    /* DO NOT NEED:
    static StartUp()
    {
      var harmony = new Harmony("GiveMeMeat.neceros");
      harmony.PatchAll(Assembly.GetExecutingAssembly());

      harmony.Patch(
        original: AccessTools.Method(type: typeof(Pawn), name: nameof(Pawn.ButcherProducts)), 
        prefix: null, 
        postfix: new HarmonyMethod(typeof(StartUp), nameof(MoreMeatPlease)));
    }

    public static IEnumerable<Thing> MoreMeatPlease(IEnumerable<Thing> __result)
    {
      foreach (var thing in __result)
      {
        if (thing.def.IsMeat)
        {
          thing.stackCount = (thing.stackCount * GiveMeMeatSettings.MeatAmountToMultiply);
          yield return thing;
        }   

        yield return thing;
      }
    } */


    /*
    public static IEnumerable<CodeInstruction> ChangeCorpseDisplay(IEnumerable<CodeInstruction> instructions)
    {
      List<CodeInstruction> instructionList = instructions.ToList();
      int amountToMul = GiveMeMeatSettings.MeatAmountToMultiply;

      for (int i = 0; i < instructionList.Count; i++)
      {
        CodeInstruction instruction = instructionList[i];

        if (instruction.opcode == OpCodes.Stloc_S &&
            instruction.OperandIs(AccessTools.Method(type: typeof(Corpse), name: nameof(Corpse.SpecialDisplayStats))))
        {
          Log.Message("GMM: Corpse Patch");
          yield return new CodeInstruction(opcode: OpCodes.Ldc_I4, operand: amountToMul);
          yield return new CodeInstruction(opcode: OpCodes.Mul);
        }

        yield return instruction;
      }
    } */
  }
}
