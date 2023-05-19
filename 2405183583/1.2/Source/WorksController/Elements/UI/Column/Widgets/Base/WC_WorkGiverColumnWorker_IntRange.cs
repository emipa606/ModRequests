using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace WorksController
{
    public abstract class WC_WorkGiverColumnWorker_IntRange : WC_WorkGiverColumnWorker_Inputable<WC_DataObject, WC_DataObject.IntRangeWrapper>
    {

        protected override float OptimalWidthFixed => 200f;

        protected abstract void SetValue(WC_DataObject data, WC_DataObject.IntRangeWrapper value, int index);

        protected abstract WC_DataObject.IntRangeWrapper GetValue(WC_DataObject data, int index);

        public override void DoHeader(Rect rect, WC_WorkGiverTable table)
        {
            base.DoHeader(rect, table);
            MouseoverSounds.DoRegion(rect);
        }

        public override void DoCell(Rect rect, WC_DataObject data, WC_WorkGiverTable table)
        {
            Text.Anchor = CellTextAnchor;
            int num = (int)((rect.width - 24f) / 2f);
            int num2 = 0;//Mathf.Max(3, 0);
            float width = OptimalWidthFixed;

            if (data.SkillDefs.Count > 1)
            {
                for (int i = 0; i < data.SkillDefs.Count; i++)
                {
                    float marginTop = 30f * i;
                    float marginLeft = Math.Abs(rect.width - width) / 2;
                    Rect rect2 = new Rect(rect.x + marginLeft, rect.y + (float)num2 + marginTop, width, 30f);
                    WC_DataObject.IntRangeWrapper value = this.GetValue(data, i);
                    IntRange prevValue = new IntRange(value?.Range.min ?? WC_DataObject.DEFAULT_SKILL_MIN, value?.Range.max ?? WC_DataObject.DEFAULT_SKILL_MAX);

                    Widgets.IntRange(rect2, data.GetHashCode() + i, ref value.Range, WC_DataObject.DEFAULT_SKILL_MIN, WC_DataObject.DEFAULT_SKILL_MAX);
                    if (Mouse.IsOver(rect2))
                    {
                        string tip = this.GetTip(data);
                        if (!tip.NullOrEmpty())
                        {
                            TooltipHandler.TipRegion(rect2, tip);
                        }
                    }
                    if (value != null && value.Range != prevValue)
                    {
                        this.NotifyOnly(data);
                    }
                }

            }
            else
            {
                float marginLeft = Math.Abs(rect.width - width) / 2;
                Rect rect2 = new Rect(rect.x + marginLeft, rect.y + (float)num2, width, 30f);
                WC_DataObject.IntRangeWrapper value = this.GetValue(data, 0);
                IntRange prevValue = new IntRange(value?.Range.min ?? WC_DataObject.DEFAULT_SKILL_MIN, value?.Range.max ?? WC_DataObject.DEFAULT_SKILL_MAX);

                Widgets.IntRange(rect2, data.GetHashCode(), ref value.Range, WC_DataObject.DEFAULT_SKILL_MIN, WC_DataObject.DEFAULT_SKILL_MAX);
                if (Mouse.IsOver(rect))
                {
                    string tip = this.GetTip(data);
                    if (!tip.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(rect2, tip);
                    }
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                }
                if (value != null && value.Range != prevValue)
                {
                    this.NotifyOnly(data);
                }
            }


        }

        public override int GetMinWidth(WC_WorkGiverTable table)
        {
            int minWidth = 100;
            if (this.def.widthPriority > 0)
            {
                minWidth = Math.Min(this.def.widthPriority, minWidth);
            }
            return Mathf.Max(base.GetMinWidth(table), minWidth);
        }

        public override int GetMaxWidth(WC_WorkGiverTable table)
        {
            return Mathf.Min(base.GetMaxWidth(table), this.GetMinWidth(table));
        }

        public override int GetMinCellHeight(WC_DataObject data)
        {
            return Mathf.Max(base.GetMinCellHeight(data), 24);
        }

        public override int Compare(WC_DataObject a, WC_DataObject b)
        {
            return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
        }

        protected abstract int GetValueToCompare(WC_DataObject data);

        protected virtual string GetTip(WC_DataObject data)
        {
            return null;
        }

        public const int HorizontalPadding = 2;

    }
}
