using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Price_Is_What
{
    [StaticConstructorOnStartup]
    public static class TooltipGiverListPatches
    {
        [HarmonyPatch(typeof(TooltipGiverList))]
        [HarmonyPatch("DispenseAllThingTooltips", MethodType.Normal)]
        public static class TooltipGiverList_DispenseAllThingTooltips
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!PlaySettingsPatches.PlaySettings_DoPlaySettingsGlobalControls.showPrice || Event.current.type != EventType.Repaint || Find.WindowStack.FloatMenu != null || Find.CurrentMap == null)
                {
                    return;
                }

                Map map = Find.CurrentMap;
                CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
                float cellSizePixels = Find.CameraDriver.CellSizePixels;
                Vector2 vector = new Vector2(cellSizePixels, cellSizePixels);
                Rect rect = new Rect(0f, 0f, vector.x, vector.y);

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (!currentViewRect.Contains(thing.Position) || thing.Position.Fogged(thing.Map))
                    {
                        continue;
                    }
                    Vector2 vector2 = thing.DrawPos.MapToUIPosition();
                    rect.x = vector2.x - vector.x / 2f;
                    rect.y = vector2.y - vector.y / 2f;
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (!thing.def.hasTooltip)
                        {
                            TipSignal tooltip = GetTooltip(thing);
                            TooltipHandler.TipRegion(rect, tooltip);
                        }
                    }
                }
            }

            private static TipSignal GetTooltip(Thing thing)
            {
                string text = thing.LabelCap;
                if (thing.def.useHitPoints)
                {
                    text = text + "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
                }
                if (thing.stackCount > 1)
                    text = text + "\n" + "MarketValueTip".Translate() + " " + thing.MarketValue + " * " + thing.stackCount + " = " + (thing.MarketValue * thing.stackCount);
                else
                    text = text + "\n" + "MarketValueTip".Translate() + " " + thing.MarketValue;

                return new TipSignal(text, thing.thingIDNumber * 251235);
            }
        }
    }
}
