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
    public class WC_WorkGiverColumnWorker_Enable : WC_WorkGiverColumnWorker_Checkbox
    {

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override float MarginTopSingle(WC_DataObject data, float rowHeight)
        {
            return Math.Max((data.SkillDefs.Count - 1) * rowHeight / data.SkillDefs.Count, 0f);
        }

        protected override bool GetValue(WC_DataObject data)
        {
            return data.Enable;
        }

        protected override void SetValue(WC_DataObject data, bool value)
        {
            if (value == this.GetValue(data))
            {
                return;
            }
            data.Enable = value;
        }

        protected override string GetTip(WC_DataObject data)
        {
            return "WC_ColumnEnableDesc".Translate();
        }

    }
}
