using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UINotIncluded
{
    [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs")]
    internal class MapInterfaceOnGUI_BeforeMainTabsPatch
    {
        public static void Prefix()
        {
            UIManager.Before_MainUIOnGUI();
        }
    }

    [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs")]
    internal class MapInterfaceOnGUI_AfterMainTabsPatch
    {
        public static void Postfix()
        {
            UIManager.After_MainUIOnGUI();
        }
    }

    [HarmonyPatch(typeof(MainButtonsRoot), "MainButtonsOnGUI")]
    internal class MainButtonRootPatch
    {
        public static void Postfix()
        {
            UIManager.MainUIOnGUI();
        }
    }

    [HarmonyPatch(typeof(MainButtonsRoot), "MainButtonsOnGUI")]
    internal class MainButtonsRoot_TranspilerPatch
    {
        private static readonly MethodInfo vanilla_DoButtons = AccessTools.Method(typeof(MainButtonsRoot), "DoButtons");
        private static MethodInfo uini_DoButtons = AccessTools.Method(typeof(UIManager), "BarsOnGUI");

        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen,
            IEnumerable<CodeInstruction> instructions)
        {
            // Detect VUIE to change the DoButtons function
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Vanilla UI Expanded"))
                uini_DoButtons = AccessTools.Method(typeof(UIManager), "VUIE_BarsOnGUI");


            var found = false;
            var patched = false;
            var temp = new CodeInstruction(OpCodes.Nop);

            foreach (var code in instructions)
            {
                if (!patched)
                {
                    if (!found)
                    {
                        if (code.opcode == OpCodes.Ldarg_0)
                        {
                            temp = code;
                            found = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == vanilla_DoButtons)
                        {
                            code.operand = uini_DoButtons;
                            code.labels = temp.labels;
                            patched = true;
                        }
                        else
                        {
                            found = false;
                            yield return temp;
                        }
                    }
                }

                yield return code;
            }
        }
    }
}