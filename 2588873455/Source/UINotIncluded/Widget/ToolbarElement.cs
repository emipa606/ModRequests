using UINotIncluded.Widget.Configs;

namespace UINotIncluded.Widget
{
    public class ToolbarElement
    {
        public readonly ElementConfig config;
        private WidgetWorker _worker;

        public ToolbarElement(ElementConfig config)
        {
            this.config = config;
        }

        public bool Configurable => config.Configurable;

        public WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = config.Worker;
                return _worker;
            }
        }
    }
}