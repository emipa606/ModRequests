using UINotIncluded.Widget.Workers;
using Verse;

namespace UINotIncluded.Widget.Configs
{
    public class TimeConfig : ElementConfig
    {
        private Time_Worker _worker;
        public ClockFormat clockFormat = ClockFormat.twentyfourHours;
        public DateFormat dateFormat = DateFormat.ddmmmYYYY;
        public RoundHour roundHour = RoundHour.tenMinute;
        public override string SettingLabel => "Time Widget";
        public override bool Configurable => true;

        public override WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = new Time_Worker(this);
                return _worker;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref dateFormat, "dateFormat", DateFormat.ddmmmYYYY);
            Scribe_Values.Look(ref roundHour, "roundHour", RoundHour.tenMinute);
            Scribe_Values.Look(ref clockFormat, "clockFormat", ClockFormat.twentyfourHours);
        }
    }
}