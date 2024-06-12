#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace NightVision.Harmony
{
    [HarmonyPatch(typeof(TooltipUtility), nameof(TooltipUtility.ShotCalculationTipString))]
    public static class TooltipUtility_ShotCalculationTipString
    {
        [HarmonyTranspiler]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> ShotCalculationTipString_Transpiler(
                        IEnumerable<CodeInstruction> instructions, ILGenerator il
                    )
        {
            var instructionsList = instructions.ToList();

            var casterFldInfo = AccessTools.Field(typeof(Verb), nameof(Verb.caster));

            var signifyingMethodInfo = AccessTools.Method(typeof(ShotReport), nameof(ShotReport.GetTextReadout));

            var ourMethodCall = new CodeInstruction(
                                                    OpCodes.Call,
                                                    AccessTools.Method(
                                                                       typeof(CombatHelpers),
                                                                       nameof(CombatHelpers
                                                                                          .NightVisionTooltipElement)
                                                                      )
                                                   );

            var aimChanceGetter = AccessTools.Property(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance)).GetGetMethod();


            for (int i = 0; i < instructionsList.Count; i++)
            {
                var current = instructionsList[i];
#if HARM12
                if (current.opcode == OpCodes.Call && current.operand == signifyingMethodInfo)
                {
#else
                    if (current.Is(OpCodes.Call, signifyingMethodInfo)/*current.opcode == OpCodes.Call && current.OperandIs(signifyingMethodInfo) *//*current.operand == signifyingMethodInfo*/)
                {

#endif

                    yield return current;

                    i++;

                    if (i >= instructionsList.Count)
                    {
                        yield break;
                    }
                    //StringBuilder.Append: consumes string from current, returns ref to stringbuilder
                    var clonedInstrAppend = instructionsList[i].Clone();


                    yield return instructionsList[i];
                    //Note: top of stack is StringBuilder

                    // load the argument of static TooltipUtility.ShotCalculationTooltip: Thing target
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    // call our method(Thing target): consumes target, returns string
                    yield return ourMethodCall;
                    // top of stack is returned string, then ref to stringbuilder: we call cloned StringBuilder.append (this StringBuilder _, string _)
                    yield return clonedInstrAppend;
                }
                else
                {
                    yield return instructionsList[i];
                }
            }
        }
    }
}
