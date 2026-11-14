using UINotIncluded.Widget.Workers;

namespace UINotIncluded.Widget.Configs
{
    public class WeatherConfig : ElementConfig
    {
        private Weather_Worker _worker;
        public override string SettingLabel => "Weather Widget";

        public override WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = new Weather_Worker(this);
                return _worker;
            }
        }
    }
}