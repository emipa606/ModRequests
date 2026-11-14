using UINotIncluded.Widget.Configs;
using UINotIncluded.Windows;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget.Workers
{
    internal class BlankSpace_Worker : WidgetWorker
    {
        private readonly BlankSpaceConfig config;

        public BlankSpace_Worker(BlankSpaceConfig config)
        {
            this.config = config;
        }

        public override bool FixedWidth => config.fixedWidth;

        public override float Width => config.width;

        public override void OpenConfigWindow()
        {
            Find.WindowStack.Add(new EditBlankSpace_Window(config));
        }

        public override void OnGUI(Rect rect)
        {
        }
    }
}