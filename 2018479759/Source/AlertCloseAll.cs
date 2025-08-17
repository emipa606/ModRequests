using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CloseAll
{
    public class AlertCloseAll: Alert
    {
        public AlertCloseAll()
        {
            defaultLabel = "CloseAll".Translate();
            defaultPriority = AlertPriority.High;
            defaultExplanation = "CloseAllDesc".Translate();
        }

        protected override Color BGColor => new Color(1f, 1f, 1f, 0.2f);

        protected override void OnClick()
        {
            Find.LetterStack.LettersListForReading.ListFullCopy().Where(letter => letter.CanDismissWithRightClick).Do(letter => Find.LetterStack.RemoveLetter(letter));
        }

        public override AlertReport GetReport()
        {
            return Find.LetterStack.LettersListForReading.Any(letter => letter.CanDismissWithRightClick);
        }
    }

    
    [HarmonyPatch(typeof(Alert))]
    [HarmonyPatch("Notify_Started")]
    public static class AlertNotifyStartedPatch
    {
        private static readonly MethodInfo LevelLoad = AccessTools.Property(typeof(Time), "timeSinceLevelLoad").GetMethod;

        private static readonly MethodInfo CheckTypeInfo =
            SymbolExtensions.GetMethodInfo(() => CheckType(null));

        private static bool CheckType(Alert alert)
        {
            return alert.GetType() == typeof(AlertCloseAll);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var injected = false;
            var foundLabel = false;
            var label = new Label?();
            foreach (var instruction in instructions)
            {
                if (!foundLabel && instruction.Branches(out label)) foundLabel = true;
                if (!injected && instruction.Calls(LevelLoad) && label.HasValue)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, CheckTypeInfo);
                    yield return new CodeInstruction(OpCodes.Brtrue, label.Value);

                    injected = true;
                }
                yield return instruction;
            }
        }
    }
}