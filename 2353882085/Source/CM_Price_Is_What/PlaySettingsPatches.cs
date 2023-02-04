using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Price_Is_What
{
    [StaticConstructorOnStartup]
    public static class PlaySettingsPatches
    {
        [StaticConstructorOnStartup]
        [HarmonyPatch(typeof(PlaySettings))]
        [HarmonyPatch("DoPlaySettingsGlobalControls", MethodType.Normal)]
        public static class PlaySettings_DoPlaySettingsGlobalControls
        {
            public static readonly Texture2D ShowPriceTexture = ContentFinder<Texture2D>.Get("UI/Buttons/ShowPrice");
            public static bool showPrice = false;

            [HarmonyPostfix]
            public static void Postfix(ref WidgetRow row, bool worldView)
            {
                if (!worldView)
                    row.ToggleableIcon(ref showPrice, ShowPriceTexture, "CM_Price_Is_What_WidgetLabel".Translate(), SoundDefOf.Mouseover_ButtonToggle);
            }
        }
    }
}
