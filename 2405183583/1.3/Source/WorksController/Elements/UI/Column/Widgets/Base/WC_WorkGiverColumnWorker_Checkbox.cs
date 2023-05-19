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
    public abstract class WC_WorkGiverColumnWorker_Checkbox : WC_WorkGiverColumnWorker_Inputable<WC_DataObject, bool>
    {

        protected abstract float MarginTopSingle(WC_DataObject data, float rowHeight);

        protected override float OptimalWidthFixed => 24f;

        public override void DoHeader(Rect rect, WC_WorkGiverTable table)
        {
            base.DoHeader(rect, table);
            MouseoverSounds.DoRegion(rect);
        }

        public override void DoCell(Rect rect, WC_DataObject data, WC_WorkGiverTable table)
        {
            if (!this.HasCheckbox(data))
            {
                return;
            }
            int num = (int)((rect.width - OptimalWidthFixed) / 2f);
            int num2 = Mathf.Max(3, 0);
            Vector2 vector = new Vector2(rect.x + (float)num, rect.y + (float)num2 + MarginTopSingle(data, OptimalWidthFixed));
            Rect rect2 = new Rect(vector.x, vector.y, OptimalWidthFixed, OptimalWidthFixed);
            bool value = this.GetValue(data);
            bool flag = value;

            Widgets.Checkbox(vector, ref value, OptimalWidthFixed, false, this.def.paintable, null, null);
            if (Mouse.IsOver(rect))
            {
                string tip = this.GetTip(data);
                if (!tip.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(rect2, tip);
                }
            }
            if (value != flag)
            {
                this.SetValueAndNotify(data, value);
            }
        }

        public override int GetMinWidth(WC_WorkGiverTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 28);
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

        private int GetValueToCompare(WC_DataObject data)
        {
            if (!this.HasCheckbox(data))
            {
                return 0;
            }
            if (!this.GetValue(data))
            {
                return 1;
            }
            return 2;
        }

        protected virtual string GetTip(WC_DataObject data)
        {
            return null;
        }

        protected virtual bool HasCheckbox(WC_DataObject data)
        {
            return true;
        }

        protected override void HeaderClicked(Rect headerRect, WC_WorkGiverTable table)
        {
            base.HeaderClicked(headerRect, table);
            if (Event.current.shift)
            {
                List<WC_DataObject> datasListForReading = table.DatasListForReading;
                for (int i = 0; i < datasListForReading.Count; i++)
                {
                    if (this.HasCheckbox(datasListForReading[i]))
                    {
                        if (Event.current.button == 0)
                        {
                            if (!this.GetValue(datasListForReading[i]))
                            {
                                this.SetValueAndNotify(datasListForReading[i], true);
                            }
                        }
                        else if (Event.current.button == 1 && this.GetValue(datasListForReading[i]))
                        {
                            this.SetValueAndNotify(datasListForReading[i], false);
                        }
                    }
                }
                if (Event.current.button == 0)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
                    return;
                }
                if (Event.current.button == 1)
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                }
            }
        }

        protected override string GetHeaderTip(WC_WorkGiverTable table)
        {
            return base.GetHeaderTip(table) + "\n" + "CheckboxShiftClickTip".Translate();
        }

        public const int HorizontalPadding = 2;

    }
}
