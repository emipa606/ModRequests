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
    public class Listing_CashRegister : Listing_Standard
    {
        public void CustomCheckboxLabeled(string label, ref bool checkOn, string tooltip = null, float height = 0f, float labelPct = 1f)
        {
            float height2 = (height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct);
            Rect rect = base.GetRect(height2, labelPct);
            rect.width = Math.Min(rect.width + 24f, base.ColumnWidth);
            if (this.BoundingRectCached == null || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CustomWidget.CheckboxLabeled(rect, label, ref checkOn, false, null, null, false, false);
            }
            base.Gap(this.verticalSpacing);
        }
    }
}
