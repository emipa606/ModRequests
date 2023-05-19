using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace WorksController
{
    public abstract class WC_WorkGiverColumnWorker_Label : WC_WorkGiverColumnWorker
    {

        protected override float OptimalWidthFixed => -1f;

        protected abstract int RowCount(WC_DataObject data);

        protected abstract float MarginTopSingle(WC_DataObject data, float rowHeight);

        protected abstract string GetLabelText(WC_DataObject data, int index);

        protected abstract string GetTipText(WC_DataObject data, int index);

        protected abstract bool CreateStatusLabelText(WC_DataObject data);

        protected abstract bool CreateStatusTipText(WC_DataObject data);

        protected abstract bool IsStatusColumn { get; }

        protected string GetTip(WC_DataObject data)
        {
            return "WC_ColumnEnableDesc".Translate();
        }

        public override void DoCell(Rect rect, WC_DataObject data, WC_WorkGiverTable table)
        {

            Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            int rowCount = RowCount(data);
            float marginLeft = ColumnMarginLeft;

            if (rowCount > 1)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    float marginTop = 30f * i;
                    rect2 = new Rect(rect.x + marginLeft, rect.y + marginTop, rect.width, Mathf.Min(rect.height, 30f));
                    if (Mouse.IsOver(rect2))
                    {
                        if (IsStatusColumn)
                        {
                            TooltipHandler.TipRegion(rect2, GetTipText(data, i));
                        }
                    }
                    string str = GetLabelText(data, i);

                    Rect rect4 = rect2;

                    rect4.xMin += 3f;
                    if (rect4.width != WC_WorkGiverColumnWorker_Label.labelCacheForWidth)
                    {
                        WC_WorkGiverColumnWorker_Label.labelCacheForWidth = rect4.width;
                        WC_WorkGiverColumnWorker_Label.labelCache.Clear();
                    }
                    Text.Font = GameFont.Small;

                    Text.Anchor = CellTextAnchor;
                    Text.WordWrap = false;
                    Widgets.Label(rect4, str.Truncate(rect4.width, WC_WorkGiverColumnWorker_Label.labelCache));
                    Text.WordWrap = true;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {
                rect2 = new Rect(rect.x + marginLeft, rect.y + MarginTopSingle(data, 30f), rect.width, Mathf.Min(rect.height, 30f));
                if (Mouse.IsOver(rect))
                {
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                    if (IsStatusColumn)
                    {
                        TooltipHandler.TipRegion(rect2, GetTipText(data, 0));
                    }
                }
                string str = GetLabelText(data, 0);

                Rect rect4 = rect2;

                rect4.xMin += 3f;
                if (rect4.width != WC_WorkGiverColumnWorker_Label.labelCacheForWidth)
                {
                    WC_WorkGiverColumnWorker_Label.labelCacheForWidth = rect4.width;
                    WC_WorkGiverColumnWorker_Label.labelCache.Clear();
                }
                Text.Font = GameFont.Small;

                Text.Anchor = CellTextAnchor;
                Text.WordWrap = false;
                Widgets.Label(rect4, str.Truncate(rect4.width, WC_WorkGiverColumnWorker_Label.labelCache));
                Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;
            }





        }

        public override int GetMinWidth(WC_WorkGiverTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 40);
        }

        public override int Compare(WC_DataObject a, WC_DataObject b)
        {
            return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
        }

        private string GetValueToCompare(WC_DataObject data)
        {
            return CompareValue(data);
        }

        protected abstract string CompareValue(WC_DataObject data);

        private const int LeftMargin = 3;

        private static Dictionary<string, string> labelCache = new Dictionary<string, string>();

        private static float labelCacheForWidth = -1f;

    }
}
