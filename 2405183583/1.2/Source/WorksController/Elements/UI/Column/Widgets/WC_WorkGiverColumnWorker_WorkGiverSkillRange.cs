using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_WorkGiverSkillRange : WC_WorkGiverColumnWorker_IntRange
    {

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override void SetValue(WC_DataObject data, WC_DataObject.IntRangeWrapper value, int index)
        {
            data.SetSkillRangeValue(value.min, value.max, index);
            SetValue(data, value);
        }

        protected override int GetValueToCompare(WC_DataObject data)
        {
            int compareMin = data.SkillRangeNotChanged ? 10000 : 0;
            int compareMax = data.SkillRangeNotChanged ? 10000 : 0;
            foreach (WC_DataObject.IntRangeWrapper range in data.SkillRanges)
            {
                compareMin += range.min == WC_DataObject.DEFAULT_SKILL_MIN ? 100 : range.min;
                compareMax += range.max == WC_DataObject.DEFAULT_SKILL_MAX ? range.max * 100 : range.max;
            }
            return compareMin + compareMax;
        }

        protected override WC_DataObject.IntRangeWrapper GetValue(WC_DataObject data, int index) => data.SkillRanges[index];

        protected override WC_DataObject.IntRangeWrapper GetValue(WC_DataObject data) => data.SkillRanges[0];

        protected override void SetValue(WC_DataObject data, WC_DataObject.IntRangeWrapper value)
        {
            // Notify only
        }
    }
}
