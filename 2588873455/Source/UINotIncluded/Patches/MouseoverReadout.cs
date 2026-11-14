using HarmonyLib;
using UINotIncluded.Widget;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    [HarmonyPatch(typeof(MouseoverReadout))]
    internal class MouseoverReadoutPatches
    {
        [HarmonyPatch(nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
        private static bool Prefix(out bool __state)
        {
            if (!Settings.vanillaReadout)
            {
                __state = false;
                return false;
            }

            __state = true;

            var deltaH = Settings.TabsOnBottom ? 35f - ExtendedToolbar.Height : 35f;

            GUI.BeginGroup(new Rect(0f, 0f + deltaH, UI.screenWidth, UI.screenHeight - deltaH));
            return true;
        }

        [HarmonyPatch(nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
        private static void Postfix(bool __state)
        {
            if (__state) GUI.EndGroup();
        }
    }

    public static class MouseoverReadoutHelper
    {
        private static string indoorsUnroofedStringCached;
        private static int indoorsUnroofedStringCachedRoofCount = -1;
        private static string cachedTemperatureString;
        private static string cachedTemperatureStringForLabel;
        private static float cachedTemperatureStringForTemperature;
        private static TemperatureDisplayMode cachedTemperatureDisplayMode;

        public static string TemperatureString()
        {
            var intVec3_1 = UI.MouseCell();
            var c = intVec3_1;
            var room1 = intVec3_1.GetRoom(Find.CurrentMap);
            if (room1 == null)
                for (var index = 0; index < 9; ++index)
                {
                    var intVec3_2 = intVec3_1 + GenAdj.AdjacentCellsAndInside[index];
                    if (intVec3_2.InBounds(Find.CurrentMap))
                    {
                        var room2 = intVec3_2.GetRoom(Find.CurrentMap);
                        if (room2 != null && ((!room2.PsychologicallyOutdoors && !room2.UsesOutdoorTemperature) ||
                                              (!room2.PsychologicallyOutdoors &&
                                               (room1 == null || room1.PsychologicallyOutdoors)) ||
                                              (room2.PsychologicallyOutdoors && room1 == null)))
                        {
                            c = intVec3_2;
                            room1 = room2;
                        }
                    }
                }

            if (room1 == null && intVec3_1.InBounds(Find.CurrentMap))
            {
                var edifice = intVec3_1.GetEdifice(Find.CurrentMap);
                if (edifice != null)
                {
                    var cellRect = edifice.OccupiedRect();
                    cellRect = cellRect.ExpandedBy(1);
                    foreach (var clipInside in cellRect.ClipInsideMap(Find.CurrentMap))
                    {
                        room1 = clipInside.GetRoom(Find.CurrentMap);
                        if (room1 != null && !room1.PsychologicallyOutdoors)
                        {
                            c = clipInside;
                            break;
                        }
                    }
                }
            }

            string str;
            if (c.InBounds(Find.CurrentMap) && !c.Fogged(Find.CurrentMap) && room1 != null &&
                !room1.PsychologicallyOutdoors)
            {
                if (room1.OpenRoofCount == 0)
                {
                    str = "Indoors".Translate();
                }
                else
                {
                    if (indoorsUnroofedStringCachedRoofCount != room1.OpenRoofCount)
                    {
                        indoorsUnroofedStringCached = "IndoorsUnroofed".Translate() + " (" +
                                                      room1.OpenRoofCount.ToStringCached() + ")";
                        indoorsUnroofedStringCachedRoofCount = room1.OpenRoofCount;
                    }

                    str = indoorsUnroofedStringCached;
                }
            }
            else
            {
                str = "Outdoors".Translate();
            }

            var num1 = room1 == null || c.Fogged(Find.CurrentMap)
                ? Find.CurrentMap.mapTemperature.OutdoorTemp
                : room1.Temperature;
            var num2 = Mathf.RoundToInt(GenTemperature.CelsiusTo(cachedTemperatureStringForTemperature,
                Prefs.TemperatureMode));
            var num3 = Mathf.RoundToInt(GenTemperature.CelsiusTo(num1, Prefs.TemperatureMode));
            if (cachedTemperatureStringForLabel != str || num2 != num3 ||
                cachedTemperatureDisplayMode != Prefs.TemperatureMode)
            {
                cachedTemperatureStringForLabel = str;
                cachedTemperatureStringForTemperature = num1;
                cachedTemperatureString = str + " " + num1.ToStringTemperature("F0");
                cachedTemperatureDisplayMode = Prefs.TemperatureMode;
            }

            return cachedTemperatureString;
        }
    }
}