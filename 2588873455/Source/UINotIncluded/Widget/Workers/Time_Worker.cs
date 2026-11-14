using System;
using System.Text;
using RimWorld;
using UINotIncluded.Widget.Configs;
using UINotIncluded.Windows;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget.Workers
{
    public class Time_Worker : WidgetWorker
    {
        private const float extra = 50f;
        private readonly TimeConfig config;
        private float _dateWidth = -1;

        private DateFormat _lastFormat;
        private float _timeWidth = -1;
        private GameFont fontCache;

        public new Func<bool> Visible = () => Find.CurrentMap != null;

        public Time_Worker(TimeConfig config)
        {
            this.config = config;
        }

        public override float Width => TimeWidth + DateWidth + IconSize + 26;

        private float TimeWidth
        {
            get
            {
                if (_timeWidth < 0 || fontCache != Settings.fontSize)
                {
                    var font = Text.Font;
                    Text.Font = Settings.fontSize;
                    _timeWidth = (float)Math.Round(Text.CalcSize("00:00 hs").x);
                    if (fontCache != Settings.fontSize) _dateWidth = -1;
                    fontCache = Settings.fontSize;
                    Text.Font = font;
                }

                return _timeWidth;
            }
        }

        private float DateWidth
        {
            get
            {
                if (Find.CurrentMap == null) return -1;
                if (_dateWidth < 0 || fontCache != Settings.fontSize || _lastFormat != config.dateFormat)
                {
                    var font = Text.Font;
                    Text.Font = Settings.fontSize;
                    var dummyDate = config.dateFormat.GetFormated(Find.TickManager.TicksAbs, 0f);
                    _dateWidth = (float)Math.Round(Text.CalcSize(dummyDate).x);
                    if (fontCache != Settings.fontSize) _timeWidth = -1;
                    fontCache = Settings.fontSize;
                    _lastFormat = config.dateFormat;
                    Text.Font = font;
                }

                return _dateWidth;
            }
        }

        public override bool FixedWidth => true;

        public override void OpenConfigWindow()
        {
            Find.WindowStack.Add(new EditTimeWidget_Window(config));
        }

        public override void OnGUI(Rect rect)
        {
            var innerRect = new Rect(rect);
            Margins(ref innerRect);
            ExtendedToolbar.DoWidgetBackground(innerRect);
            Padding(ref innerRect);

            if (Find.CurrentMap == null) return;

            var space = innerRect.ContractedBy(ExtendedToolbar.padding);

            var pos = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            var season = GenDate.Season(Find.TickManager.TicksAbs, pos);
            var iconSpace = DrawIcon(season.GetIconTex(), space.x, space.y, season.LabelCap());
            space.x += iconSpace.width;
            space.width -= iconSpace.width;

            var row = new WidgetRow(space.x, rect.y, UIDirection.RightThenDown, gap: ExtendedToolbar.interGap);

            var hour = GenDate.HourFloat(Find.TickManager.TicksAbs, pos.x);
            int minutes;
            switch (config.roundHour)
            {
                case RoundHour.hour:
                    minutes = 0;
                    break;

                case RoundHour.tenMinute:
                    minutes = (int)Math.Floor((hour - Math.Floor(hour)) * 6) * 10;
                    break;

                case RoundHour.minute:
                    minutes = (int)Math.Floor((hour - Math.Floor(hour)) * 60);
                    break;

                default:
                    throw new NotImplementedException();
            }

            var datestamp = config.dateFormat.GetFormated(Find.TickManager.TicksAbs, pos.x);

            var dateWidth = Text.CalcSize(datestamp).x;
            var remainingSpace = space.width - dateWidth - TimeWidth;

            var dateLabelWidth = (float)Math.Floor(dateWidth + remainingSpace / 3);
            var timeLabelWidth = (float)Math.Floor(TimeWidth + remainingSpace * 2 / 3);

            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;

            row.Label(datestamp, dateLabelWidth, GetDateDescription(pos, season), rect.height);

            string timestamp;
            switch (config.clockFormat)
            {
                case ClockFormat.twelveHours:
                    var meridiam = hour > 12 ? "pm" : "am";
                    hour = hour > 12 ? hour % 12 : hour;
                    hour = (float)Math.Floor(hour);
                    hour = hour == 0 ? 12 : hour;
                    timestamp = string.Format("{0}:{1} {2}", hour.ToString(), minutes.ToString("D2"), meridiam);
                    row.Label(timestamp, timeLabelWidth, height: rect.height);
                    break;

                case ClockFormat.twentyfourHours:
                    timestamp = string.Format("{0}:{1} {2}", Math.Floor(hour).ToString(), minutes.ToString("D2"), "hs");
                    row.Label(timestamp, timeLabelWidth, height: rect.height);
                    break;

                case ClockFormat.vanilla:
                    DateReadout.DateOnGUI(new Rect(row.FinalX, row.FinalY, timeLabelWidth, rect.height));
                    break;

                default:
                    throw new NotImplementedException();
            }

            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static string GetDateDescription(Vector2 pos, Season season)
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < 4; ++i)
            {
                var quadrum = (Quadrum)i;
                stringBuilder.AppendLine(quadrum.Label() + " - " + quadrum.GetSeason(pos.y).LabelCap());
            }

            var description = "DateReadoutTip".Translate(GenDate.DaysPassed, 15, (NamedArgument)season.LabelCap(), 15,
                (NamedArgument)GenDate.Quadrum(GenTicks.TicksAbs, pos.x).Label(),
                (NamedArgument)stringBuilder.ToString());
            return description.ToString();
        }
    }
}