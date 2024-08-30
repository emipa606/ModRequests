using RimWorld;
using RimWorld.Planet;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Revolus.MoreAutosaveSlots {
    public class MoreAutosaveSlotsSettings : ModSettings {
        private static readonly int countDefault = 5;
        private static readonly int countMin = 1;
        private static readonly int countMax = 60;
        public int count = countDefault;

        private static readonly string formatDefault = "{faction} ({index})";
        public string format = formatDefault;

        private static readonly int hoursDefault = 0;
        private static readonly int hoursMin = 0;
        private static readonly int hoursMax = 15 * 24;
        public int hours = hoursDefault;

        private static readonly bool useNextSaveNameDefault = true;
        public bool useNextSaveName = useNextSaveNameDefault;

        public override void ExposeData() {
            Scribe_Values.Look(ref this.count, "count", countDefault);
            Scribe_Values.Look(ref this.format, "format", formatDefault);
            Scribe_Values.Look(ref this.hours, "hours", hoursDefault);
            Scribe_Values.Look(ref this.useNextSaveName, "useNextSaveName", useNextSaveNameDefault);
            base.ExposeData();
        }

        public static string[] AutoSaveNames(bool onlyOne = false) {
            var settings = MoreAutosaveSlotsMod.Settings;
            var count = Math.Max(Math.Min(settings.count, countMax), countMin);
            var now = DateTime.Now;

            string faction = null;
            try { faction = Faction.OfPlayerSilentFail?.Name; } catch { }
            if (string.IsNullOrEmpty(faction)) {
                faction = "MoreAutosaveSlots.FactionNameDefault".Translate();
            }

            string settlement = null;
            try { settlement = ((Settlement) Find.CurrentMap.info.parent).Name; } catch { }
            if (string.IsNullOrEmpty(settlement)) {
                settlement = "MoreAutosaveSlots.SettlementNameDefault".Translate();
            }

            var almostFormatted = (
                settings.format.
                Replace("{faction}", faction).
                Replace("{settlement}", settlement).
                Replace("{year}", now.Year.ToString("D4")).
                Replace("{month}", now.Month.ToString("D2")).
                Replace("{day}", now.Day.ToString("D2")).
                Replace("{hour}", now.Hour.ToString("D2")).
                Replace("{minute}", now.Minute.ToString("D2")).
                Replace("{second}", now.Second.ToString("D2")).
                Trim()
            );

            string doFormat(string s, int index) {
                s = GenFile.SanitizedFileName(s.Replace("{index}", index.ToString()));
                if (s.Length > 60) {
                    s = s.Substring(0, 60).Trim();
                }
                return s;
            }

            if (onlyOne) {
                return new string[] { doFormat(almostFormatted, count) };
            }

            var result = new string[count];
            for (var i = 0; i < count; i++) {
                result[i] = doFormat(almostFormatted, i + 1);
            }
            return result;
        }

        public static string NextName() {
            var texts = AutoSaveNames();

            var text = (from name in texts where !SaveGameFilesUtility.SavedGameNamedExists(name) select name).FirstOrDefault();
            if (!(text is null)) {
                return text;
            }

            return texts.MinBy((string name) => new FileInfo(GenFilePaths.FilePathForSavedGame(name)).LastWriteTime);
        }

        internal bool ShowAndChangeSettings(Listing_Standard listing) {
            void line(string left, string right) {
                Rect wholeRect = listing.GetRect(Text.LineHeight);

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(wholeRect.LeftHalf().Rounded(), left);
                
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(wholeRect.RightHalf().Rounded(), right);
                
                listing.Gap(listing.verticalSpacing);
            }

            int countNew, countOld = this.count;
            string formatNew, formatOld = this.format;
            int hoursNew, hoursOld = this.hours;
            bool useNextSaveNameNew, useNextSaveNameOld = this.useNextSaveName;

            var reset = Widgets.ButtonText(listing.GetRect(Text.LineHeight).RightHalf().LeftHalf().Rounded(), "MoreAutosaveSlots.ButtonReset".Translate());
            listing.Gap(listing.verticalSpacing + Text.LineHeight);

            line("MoreAutosaveSlots.LabelExample".Translate(), AutoSaveNames(true)[0]);
            listing.Gap(Text.LineHeight);
            
            {
                Rect wholeRect = listing.GetRect(2 * Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect dataRect = wholeRect.RightHalf().Rounded();
                Rect dataDescRect = dataRect.TopHalf().Rounded();
                Rect dataSliderRect = dataRect.BottomHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "MoreAutosaveSlots.LabelCount".Translate());

                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(dataDescRect, countOld == 1 ? "MoreAutosaveSlots.CountOne".Translate() : "MoreAutosaveSlots.CountMultiple".Translate(countOld));
                Text.Anchor = TextAnchor.UpperCenter;
                countNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSliderRect, countOld, countMin, countMax));

                listing.Gap(listing.verticalSpacing + Text.LineHeight);
            }
            {
                Rect wholeRect = listing.GetRect(Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect dataRect = wholeRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "MoreAutosaveSlots.LabelFormat".Translate());

                Text.Anchor = TextAnchor.MiddleCenter;
                formatNew = Widgets.TextArea(dataRect.Rounded(), formatOld);
                listing.Gap(listing.verticalSpacing);

                foreach (Match m in Regex.Matches("MoreAutosaveSlots.FormatDescription".Translate(), @"\|([^|]*)\|([^|]*)\|\|")) {
                    var groups = m.Groups;
                    line(groups[1].Value, groups[2].Value);
                }
                
                listing.Gap(Text.LineHeight);
            }
            {
                Rect wholeRect = listing.GetRect(2 * Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect dataRect = wholeRect.RightHalf().Rounded();
                Rect dataDescRect = dataRect.TopHalf().Rounded();
                Rect dataSliderRect = dataRect.BottomHalf().Rounded();

                string intervalDescription;
                if (hoursOld <= 0) {
                    intervalDescription = "MoreAutosaveSlots.HoursDisabled".Translate();
                } else {
                    var days = hoursOld / 24;
                    var hours = hoursOld % 24;
                    if (hours == 0) {
                        if (days == 1) {
                            intervalDescription = "MoreAutosaveSlots.HoursDaily".Translate();
                        } else {
                            intervalDescription = "MoreAutosaveSlots.HoursDays".Translate(days);
                        }
                    } else if (days == 0) {
                        intervalDescription = "MoreAutosaveSlots.HoursHours".Translate(hours);
                    } else {
                        intervalDescription = "MoreAutosaveSlots.HoursDaysAndDays".Translate(days, hours);
                    }
                }

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "MoreAutosaveSlots.LabelHours".Translate());

                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(dataDescRect, intervalDescription);
                Text.Anchor = TextAnchor.UpperCenter;
                hoursNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSliderRect, hoursOld, hoursMin, hoursMax) / 3) * 3;
                
                listing.Gap(listing.verticalSpacing + Text.LineHeight);
            }
            {
                Rect wholeRect = listing.GetRect(Text.LineHeight);
                Rect labelRect = wholeRect.LeftHalf().Rounded();
                Rect dataRect = wholeRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "MoreAutosaveSlots.LabelUseNextSaveName".Translate());

                Text.Anchor = TextAnchor.MiddleLeft;
                useNextSaveNameNew = useNextSaveNameOld;
                Widgets.CheckboxLabeled(dataRect, "", ref useNextSaveNameNew, placeCheckboxNearText: true);
                
                listing.Gap(listing.verticalSpacing);
            }

            if (reset) {
                countNew = countDefault;
                formatNew = formatDefault;
                hoursNew = hoursDefault;
                useNextSaveNameNew = useNextSaveNameDefault;
            }

            var changed = false;
            if (countNew != countOld) {
                this.count = countNew;
                changed = true;
            }
            if (formatNew != formatOld) {
                this.format = formatNew;
                changed = true;
            }
            if (hoursNew != hoursOld) {
                this.hours = hoursNew;
                changed = true;
            }
            if (useNextSaveNameNew != useNextSaveNameOld) {
                this.useNextSaveName = useNextSaveNameNew;
                changed = true;
            }
            return changed;
        }
    }
}
