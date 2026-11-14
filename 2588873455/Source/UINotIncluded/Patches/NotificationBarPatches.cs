using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UINotIncluded.Widget;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    [HarmonyPatch(typeof(GlobalControls), "GlobalControlsOnGUI")]
    internal class GlobalControl_TranspilerPatch
    {
        private static readonly MethodInfo utility_DoDate = AccessTools.Method(typeof(GlobalControlsUtility), "DoDate");

        private static readonly MethodInfo globalControls_Temperaturestring =
            AccessTools.Method(typeof(GlobalControls), "TemperatureString");

        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen,
            IEnumerable<CodeInstruction> instructions)
        {
            var prev = instructions.First();
            var patchWeater = false;
            var patchWeater_finish = false;
            var patchedTemp = false;

            foreach (var code in instructions)
                if (!patchWeater)
                {
                    if (prev.opcode == OpCodes.Call && (MethodInfo)prev.operand == utility_DoDate)
                    {
                        patchWeater = true;

                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(GlobalControl_TranspilerPatch), nameof(DoWeather)));
                        continue;
                    }

                    yield return code;
                    prev = code;
                }
                else if (!patchWeater_finish)
                {
                    if (prev.opcode == OpCodes.Newobj)
                    {
                        yield return code;
                        patchWeater_finish = true;
                    }

                    prev = code;
                }
                else if (!patchedTemp)
                {
                    if (prev.opcode == OpCodes.Call && (MethodInfo)prev.operand == globalControls_Temperaturestring)
                    {
                        patchedTemp = true;

                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(GlobalControl_TranspilerPatch), nameof(DoTemperature)));
                        continue;
                    }

                    yield return code;
                    prev = code;
                }
                else
                {
                    yield return code;
                }
        }

        private static void DoWeather(float x, ref float y)
        {
            if (!Settings.vanillaWeather) return;
            y -= 26f;
            var rect = new Rect(x - 22f, y, 230f, 26f);
            Find.CurrentMap.weatherManager.DoWeatherGUI(rect);
        }

        private static void DoTemperature(string label, float x, ref float y)
        {
            if (!Settings.vanillaTemperature) return;
            y -= 26f;
            var rect = new Rect(x - 100f, y, 293f, 26f);
            Widgets.Label(rect, label);
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility))]
    internal class GlobalControlsUtilityPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoPlaySettings))]
        [HarmonyPriority(Priority.Low)]
        private static bool DoPlaySettingsPatch(WidgetRow rowVisibility, bool worldView, ref float curBaseY)
        {
            if (Settings.TabsOnBottom) curBaseY -= UIManager.ExtendedBarHeight;
            if (Settings.useDesignatorBar && !Settings.designationsOnLeft && !worldView) curBaseY -= 88f;
            if (Settings.togglersOnTop) curBaseY += ExtendedToolbar.Height;
            var borderGap = 4f;
            var initialY = Settings.togglersOnTop
                ? Settings.TabsOnTop ? UIManager.ExtendedBarHeight + borderGap : borderGap
                : curBaseY;
            rowVisibility.Init(UI.screenWidth - borderGap, initialY,
                Settings.togglersOnTop ? UIDirection.LeftThenDown : UIDirection.LeftThenUp,
                Settings.togglersOnTop ? 250f : 180f);
            Find.PlaySettings.DoPlaySettingsGlobalControls(rowVisibility, worldView);
            if (!Settings.togglersOnTop) curBaseY = rowVisibility.FinalY;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoTimespeedControls))]
        [HarmonyPriority(Priority.Low)]
        private static bool DoTimespeedControls_Patch(ref float curBaseY)
        {
            if (Settings.vanillaControlSpeed) return true;
            curBaseY += 4f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoDate))]
        [HarmonyPriority(Priority.Low)]
        private static bool DoDate_Patch()
        {
            return Settings.vanillaDate;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoRealtimeClock))]
        [HarmonyPriority(Priority.Low)]
        private static bool DoRealtimeClock_Patch(ref float curBaseY)
        {
            if (Settings.vanillaRealtime) return true;
            curBaseY += 4f;
            return false;
        }
    }
}