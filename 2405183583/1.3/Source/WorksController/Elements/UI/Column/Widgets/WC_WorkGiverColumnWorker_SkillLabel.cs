using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_SkillLabel : WC_WorkGiverColumnWorker_Label
    {

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override bool IsStatusColumn => false;

        protected override string CompareValue(WC_DataObject data)
        {
            return data.PrimarySkillLabel;
            //return int.MaxValue - WorkGiverUtil.StringCompareValue(data.PrimarySkillLabel);
            ////int compare = data.PrimarySkillLabel.GetHashCode();
            ////return compare;
        }

        protected override bool CreateStatusLabelText(WC_DataObject data) => false;

        protected override bool CreateStatusTipText(WC_DataObject data) => false;

        protected override string GetLabelText(WC_DataObject data, int index)
        {
            if (data.SkillDefs.Count > index)
            {
                return data.SkillDefs[index].LabelCap;
            }
            return null;
        }

        protected override string GetTipText(WC_DataObject data, int index) => null;

        protected override float MarginTopSingle(WC_DataObject data, float rowHeight)
        {
            return 0f;
        }

        protected override int RowCount(WC_DataObject data)
        {
            return data.SkillDefs.Count;
        }

    }
}
