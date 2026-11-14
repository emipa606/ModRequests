using UINotIncluded.Widget.Workers;
using Verse;

namespace UINotIncluded.Widget.Configs
{
    internal class BlankSpaceConfig : ElementConfig
    {
        private BlankSpace_Worker _worker;
        public bool fixedWidth;
        public float width = 100f;
        public override bool Configurable => true;
        public override string SettingLabel => "Blank Space";

        public override bool Repeatable => true;

        public override WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = new BlankSpace_Worker(this);
                return _worker;
            }
        }

        public override void Reset()
        {
            fixedWidth = false;
            width = 100f;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fixedWidth, "fixedWidth");
            Scribe_Values.Look(ref width, "width", 100f);
        }
    }
}