using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CashRegister
{
    [StaticConstructorOnStartup]
    public static class CustomWidget
    {
        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, bool paintable = false)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Rect rect2 = rect;
            rect2.xMax -= 24f;
            Widgets.Label(rect2, label);
            if (!disabled)
            {
                Widgets.ToggleInvisibleDraggable(rect, ref checkOn, true, paintable);
            }
            Widgets.CheckboxDraw(rect.x + rect.width - 48f, rect.y + (rect.height - 24f) / 2f, checkOn, disabled, 24f, texChecked, texUnchecked);
            Text.Anchor = anchor;
        }
    }
}
