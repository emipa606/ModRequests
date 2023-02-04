using System.Globalization;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CM_Price_Is_What
{
    [StaticConstructorOnStartup]
    public static class GlobalControlsUtilityPatches
    {
        [HarmonyPatch(typeof(GlobalControlsUtility))]
        [HarmonyPatch("DoPlaySettings", MethodType.Normal)]
        public static class GlobalControlsUtility_DoPlaySettings
        {
            [HarmonyPostfix]
            public static void Postfix(bool worldView, ref float curBaseY)
            {
                if (Find.CurrentMap == null || worldView || !PlaySettingsPatches.PlaySettings_DoPlaySettingsGlobalControls.showPrice)
                    return;

                float width = 200.0f;
                float leftX = (float)UI.screenWidth - width;
                Rect wealthRect = new Rect(leftX - 20f, curBaseY - 26f, width + 20f - 7f, 26f);
                Text.Anchor = TextAnchor.MiddleRight;

                string wealthTotalString = Find.CurrentMap.wealthWatcher.WealthTotal.ToString("C", CultureInfo.CurrentCulture);
                Widgets.Label(wealthRect, wealthTotalString);
                Text.Anchor = TextAnchor.UpperLeft;
                curBaseY -= 26f;

                if (Mouse.IsOver(wealthRect))
                {
                    string wealthItemsString = Find.CurrentMap.wealthWatcher.WealthItems.ToString("C", CultureInfo.CurrentCulture);
                    string wealthBuildingsString = Find.CurrentMap.wealthWatcher.WealthBuildings.ToString("C", CultureInfo.CurrentCulture);
                    string wealthPawnsString = Find.CurrentMap.wealthWatcher.WealthPawns.ToString("C", CultureInfo.CurrentCulture);

                    TaggedString taggedString = "CM_Price_Is_What_MapWealthTooltip".Translate(wealthItemsString, wealthBuildingsString, wealthPawnsString, wealthTotalString);
                    TooltipHandler.TipRegion(wealthRect, new TipSignal(taggedString, 86424));
                }
            }
        }
    }
}
