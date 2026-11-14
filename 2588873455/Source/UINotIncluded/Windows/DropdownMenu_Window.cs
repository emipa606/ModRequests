using System.Linq;
using RimWorld;
using UINotIncluded.Widget.Configs;
using UnityEngine;
using Verse;

namespace UINotIncluded.Windows
{
    public class DropdownMenu_Window : MainTabWindow
    {
        public static DropdownMenuConfig config;

        public DropdownMenu_Window()
        {
            layer = WindowLayer.Super;
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                var x = config.matchLabelSize ? config.lastWidth : config.width;
                float n_elements = config.elements.Count();

                var y = UIManager.ExtendedBarHeight * n_elements + config.spacing * (n_elements + 1);

                return new Vector2(x, y);
            }
        }

        protected override float Margin => config.spacing;

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

            if (config.lastY > UI.screenHeight / 2)
                windowRect.y = UI.screenHeight - UIManager.ExtendedBarHeight - windowRect.height;
            else
                windowRect.y = config.lastY + UIManager.ExtendedBarHeight;

            if (config.lastX + windowRect.width > UI.screenWidth)
                windowRect.x = UI.screenWidth - windowRect.width;
            else
                windowRect.x = config.lastX;
        }

        public override void DoWindowContents(Rect rect)
        {
            GUI.BeginGroup(rect);
            var curY = 0.0f;
            Text.Font = GameFont.Small;
            foreach (ButtonConfig config in config.elements)
                curY += DrawElement(config, new Vector2(0.0f, curY), rect.width) + DropdownMenu_Window.config.spacing;

            GUI.EndGroup();
        }

        private float DrawElement(ElementConfig config, Vector2 pos, float width)
        {
            var rect = new Rect(pos.x, pos.y, width, UIManager.ExtendedBarHeight);
            config.Worker.OnGUI(rect);
            return UIManager.ExtendedBarHeight;
        }
    }
}