using System;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget.Workers
{
    internal class TimeIrl_Worker : WidgetWorker
    {
        private float cacheWidth = -1f;
        private GameFont fontCache;
        public new Func<bool> Visible = () => Prefs.ShowRealtimeClock;

        public override bool FixedWidth => true;

        private float extra => IconSize + 16f;

        public override float Width
        {
            get
            {
                if (cacheWidth < 0 || fontCache != Settings.fontSize)
                {
                    var font = Text.Font;
                    Text.Font = Settings.fontSize;
                    cacheWidth = (float)Math.Round(Text.CalcSize("00:00 hs").x);
                    fontCache = Settings.fontSize;
                    Text.Font = font;
                }

                return cacheWidth + extra;
            }
        }

        public override void OnGUI(Rect rect)
        {
            var innerRect = new Rect(rect);
            Margins(ref innerRect);
            ExtendedToolbar.DoWidgetBackground(innerRect);
            Padding(ref innerRect);

            var iconSpace = DrawIcon(ModTextures.iconWorld, innerRect.x, innerRect.y);
            innerRect.x += iconSpace.width;
            innerRect.width -= iconSpace.width;
            var row = new WidgetRow(innerRect.x, rect.y, UIDirection.RightThenDown, gap: ExtendedToolbar.interGap);

            string label;

            if (Prefs.TwelveHourClockMode)
                label = DateTime.Now.ToString("hh:mm tt");
            else
                label = DateTime.Now.ToString("HH:mm") + " hs";

            Text.Anchor = TextAnchor.MiddleLeft;
            row.Label(label, innerRect.width, null, rect.height);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}