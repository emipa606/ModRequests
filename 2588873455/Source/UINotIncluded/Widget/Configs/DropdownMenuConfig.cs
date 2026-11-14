using System;
using System.Collections.Generic;
using RimWorld;
using UINotIncluded.Widget.Workers;
using UINotIncluded.Windows;
using Verse;

namespace UINotIncluded.Widget.Configs
{
    public class DropdownMenuConfig : ButtonConfig
    {
        private Dropdown_Worker _worker;
        public List<ElementConfig> elements = new List<ElementConfig>();
        public float lastWidth = 0f;

        public float lastX = 0f;
        public float lastY = 0f;
        public bool matchLabelSize;
        public float spacing = 5f;
        public float width = 200f;

        public DropdownMenuConfig() : base(new MainButtonDef
        {
            defName = "UINI_NMB" + DateTime.Now.ToFileTime(),
            label = "",
            tabWindowClass = typeof(DropdownMenu_Window)
        })
        {
            Reset();
        }

        public override string SettingLabel => "Dropdown " + Label;

        public override WidgetWorker Worker
        {
            get
            {
                if (_worker == null) _worker = new Dropdown_Worker(this);
                return _worker;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref width, "width", 200f);
            Scribe_Values.Look(ref spacing, "spacing", 5f);
            Scribe_Values.Look(ref matchLabelSize, "matchLabelSize");
            Scribe_Collections.Look(ref elements, "elements", LookMode.Deep);
        }
    }
}