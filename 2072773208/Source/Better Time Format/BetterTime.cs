// BetterTimeFormat - BetterTime.cs
// Created on 2022.11.09
// Last modified at 2022.11.14 16:50

#region
using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;

using RimWorld;
using RimWorld.Planet;

using UnityEngine;

using Verse;
#endregion

namespace BetterTimeFormat
{
    [StaticConstructorOnStartup]
    public static class BetterTimeFormat
    {
        // Credits to Palota for the method, posted here:
        // https://stackoverflow.com/questions/24437274/string-indexof-search-for-whole-word-match/47638909#47638909
        private static int IndexOfWholeWord(this string str, string word, StringComparison comparison)
        {
            for (var j = 0; j < str.Length && (j = str.IndexOf(word, j, comparison)) >= 0; j++)
            {
                if ((j == 0 || !char.IsLetterOrDigit(str, j - 1)) && (j + word.Length == str.Length || !char.IsLetterOrDigit(str, j + word.Length)))
                    return j;
            }

            return -1;
        }

        // Credits to MethodMan for the method, posted here:
        // https://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string/8809437#8809437
        public static string ReplaceFirst(string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
                return text;

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }

    [HarmonyPatch(typeof(DateReadout), "DateOnGUI")]
    internal static class Patch_BetterTimeFormat
    {
        public static bool Prefix(Rect dateRect, ref List<string> ___seasonsCached, ref int ___dateStringDay, ref Season ___dateStringSeason, ref Quadrum ___dateStringQuadrum, ref int ___dateStringYear, ref string ___dateString)
        {
            Vector2 vector2;
            switch (WorldRendererUtility.WorldRenderedNow)
            {
            case true when Find.WorldSelector.selectedTile >= 0:
                vector2 = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
                break;

            case true when Find.WorldSelector.NumSelectedObjects > 0:
                vector2 = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
                break;

            default:
            {
                if (Find.CurrentMap == null)
                    return false;

                vector2 = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                break;
            }
            }

            var num1 = GenDate.DayOfTwelfth(Find.TickManager.TicksAbs, vector2.x);
            var season = GenDate.Season(Find.TickManager.TicksAbs, vector2);
            var quadrum1 = GenDate.Quadrum(Find.TickManager.TicksAbs, vector2.x);
            var num2 = GenDate.Year(Find.TickManager.TicksAbs, vector2.x);
            var str = true ? ___seasonsCached[(int) season] : "";
            if (num1 != ___dateStringDay || season != ___dateStringSeason || quadrum1 != ___dateStringQuadrum || num2 != ___dateStringYear)
            {
                ___dateString = GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs, vector2);
                ___dateStringDay = num1;
                ___dateStringSeason = season;
                ___dateStringQuadrum = quadrum1;
                ___dateStringYear = num2;
            }

            var userTime = "";
            if (BetterTimeFormatMod.Settings.UpdateTime)
            {
                userTime = BetterTimeFormatMod.Settings.TimeFormat;
                var dayPercent = GenLocalDate.DayPercent(Find.CurrentMap);

                if (BetterTimeFormatMod.Settings.UpdateHours)
                {
                    var hours = Math.Floor(dayPercent * 24);
                    if (BetterTimeFormatMod.Settings.TwelveHourFormat)
                        hours = dayPercent < 0.6 ? hours : hours - 12;

                    userTime = userTime.ReplaceFirst("HH", $"{hours,0:00}");
                    userTime = userTime.ReplaceFirst("H", $"{hours,0}");
                }

                if (BetterTimeFormatMod.Settings.UpdateMinutes)
                {
                    var minutes = Math.Floor(dayPercent * 24 % 1 * 60);
                    userTime = userTime.ReplaceFirst("MM", $"{minutes,0:00}");
                    userTime = userTime.ReplaceFirst("M", $"{minutes,0:0}");
                }

                if (BetterTimeFormatMod.Settings.UpdateSeconds)
                {
                    var seconds = Math.Floor(dayPercent * 24 % 1 * 60 % 1 * 60);
                    userTime = userTime.ReplaceFirst("SS", $"{seconds,0:00}");
                    userTime = userTime.ReplaceFirst("S", $"{seconds,0:0}");
                }

                if (BetterTimeFormatMod.Settings.TwelveHourFormat)
                {
                    var notation = dayPercent < 0.5 ? BetterTimeFormatMod.Settings.AmString : BetterTimeFormatMod.Settings.PmString;
                    userTime = userTime.ReplaceFirst("N", notation);
                }
            }

            Text.Font = GameFont.Small;
            var num3 = Mathf.Max(Mathf.Max(Text.CalcSize(userTime).x, Text.CalcSize(___dateString).x), Text.CalcSize(str).x) + 7f;
            dateRect.xMin = dateRect.xMax - num3;
            if (Mouse.IsOver(dateRect))
                Widgets.DrawHighlight(dateRect);

            GUI.BeginGroup(dateRect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperRight;
            var rect = dateRect.AtZero();
            rect.xMax -= 7f;
            Widgets.Label(rect, userTime);
            rect.yMin += 26f;
            Widgets.Label(rect, ___dateString);
            rect.yMin += 26f;
            if (!str.NullOrEmpty())
                Widgets.Label(rect, str);

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            if (!Mouse.IsOver(dateRect))
                return false;

            var stringBuilder = new StringBuilder();
            for (var index2 = 0; index2 < 4; ++index2)
            {
                var quadrum2 = (Quadrum) index2;
                stringBuilder.AppendLine(quadrum2.Label() + " - " + quadrum2.GetSeason(vector2.y).LabelCap());
            }

            var taggedString = "DateReadoutTip".Translate(GenDate.DaysPassed, 15, (NamedArgument) season.LabelCap(), 15, (NamedArgument) GenDate.Quadrum(GenTicks.TicksAbs, vector2.x).Label(), (NamedArgument) stringBuilder.ToString());
            TooltipHandler.TipRegion(dateRect, new TipSignal(taggedString, 86423));

            return false;
        }
    }
}
