using System.Collections.Generic;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget
{
    internal static class ExtendedToolbar
    {
        public static readonly float interGap = 0;
        public static readonly float padding = 2;
        public static readonly float margin = 3;
        public static float Height => Settings.barsHeight;
        public static float Width => UI.screenWidth;

        public static void ExtendedToolbarOnGUI(List<ElementConfig> elements, Rect inRect)
        {
#if DEBUG
            const string key = "Draw Toolbar";
            Analyzer.Profiling.ProfileController.Start(key);
#endif

            if (!elements.Any()) return;
            Settings.BarStyle.DoToolbarBackground(inRect);

            var fixedWidth = 0f;
            var elasticElementsAmount = 0;

            foreach (var element in elements)
            {
                if (!element.Worker.Visible()) continue;
                if (!element.Worker.FixedWidth) elasticElementsAmount++;
                else fixedWidth += element.Worker.Width;
            }

            var elasticSpaceAvailable = Width - fixedWidth;
            var elasticElementWidth = elasticSpaceAvailable / elasticElementsAmount;

            float curX = 0;
            foreach (var element in elements)
            {
                if (!element.Worker.Visible()) continue;
                var eWidth = element.Worker.Width;
                if (!element.Worker.FixedWidth) eWidth = elasticElementWidth;

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = Settings.fontSize;
                Text.WordWrap = false;
                element.Worker.OnGUI(new Rect(curX, inRect.y, eWidth, Height));
                Text.WordWrap = true;
                curX += eWidth;
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
#if DEBUG
            Analyzer.Profiling.ProfileController.Stop(key);
#endif
        }

        public static void DoWidgetBackground(Rect rect)
        {
            Settings.BarStyle.DoWidgetBackground(rect);
        }
    }

    public enum ToolbarPosition
    {
        top,
        bottom
    }
}