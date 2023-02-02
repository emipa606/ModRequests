using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BlueprintMaterialDebt
{
    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls), new Type[] { typeof(WidgetRow), typeof(bool) })]
    static class PlaySettings_DoPlaySettingsGlobalControls_Patch
    {
        static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
                return;

            if (row == null || BlueprintMaterialDebt.ToggleIcon == null)
                return;

            row.ToggleableIcon(
                ref BlueprintMaterialDebt.SubtractResources,
                BlueprintMaterialDebt.ToggleIcon,
                "BlueprintMaterialDebt_toggleTip".Translate(),
                SoundDefOf.Mouseover_ButtonToggle
            );
        }
    }
}
