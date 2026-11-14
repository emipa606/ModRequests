using System;
using UnityEngine;
using Verse;

namespace UINotIncluded.Widget
{
    public abstract class WidgetWorker
    {
        public Func<bool> Visible = () => { return true; };
        public float IconSize => ExtendedToolbar.Height - 9f;
        public virtual bool FixedWidth => false;
        public virtual float Width => 100f;

        public Rect DrawIcon(Texture2D icon, float curX, float curY, string tooltip = null)
        {
            var rect = new Rect(curX, curY, IconSize, IconSize);
            GUI.DrawTexture(rect, icon);
            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
            return rect;
        }

        public virtual void Margins(ref Rect rect)
        {
            rect = rect.ContractedBy(ExtendedToolbar.margin);
        }

        public abstract void OnGUI(Rect rect);

        public virtual void OpenConfigWindow()
        {
        }

        public virtual void Padding(ref Rect rect)
        {
            rect = rect.ContractedBy(ExtendedToolbar.padding);
        }
    }
}