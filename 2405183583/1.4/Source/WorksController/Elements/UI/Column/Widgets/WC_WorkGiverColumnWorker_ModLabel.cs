using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_ModLabel : WC_WorkGiverColumnWorker_Label
    {

        protected override float HeaderMarginLeft => 5f;

        protected override float ColumnMarginLeft => 5f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleLeft;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleLeft;

        protected override bool IsStatusColumn => false;

        protected override string CompareValue(WC_DataObject data)
        {
            return data.ModLabel;
            //return int.MaxValue - WorkGiverUtil.StringCompareValue(data.ModLabel);
            ////int compare = data.ModLabel.GetHashCode();
            ////return compare;
        }

        protected override bool CreateStatusLabelText(WC_DataObject data) => false;

        protected override bool CreateStatusTipText(WC_DataObject data) => false;

        protected override string GetLabelText(WC_DataObject data, int index) => data.ModLabel;

        protected override string GetTipText(WC_DataObject data, int index) => null;

        protected override float MarginTopSingle(WC_DataObject data, float rowHeight)
        {
            return Math.Max((data.SkillDefs.Count - 1) * rowHeight / data.SkillDefs.Count, 0f);
        }

        protected override int RowCount(WC_DataObject data)
        {
            return 1;
        }

    }
}
