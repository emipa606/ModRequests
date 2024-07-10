using System;
using UnityEngine;
using Verse;

namespace TerrainOverhaul
{
    public class PopupWindow : Window
    {
        public Action<Rect> Draw;

        public static void Open(Action<Rect> draw)
        {
            Find.WindowStack.Add(new PopupWindow(){Draw = draw});
        }

        private PopupWindow()
        {
            base.drawShadow = true;
            base.draggable = false;
            base.doCloseX = false;
            base.doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Draw == null)
            {
                Close(false);
                return;
            }

            Draw(new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 100));
        }
    }
}
