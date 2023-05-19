using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_WorkGiverLabel : WC_WorkGiverColumnWorker_Label
    {

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override bool IsStatusColumn => false;

        protected override string CompareValue(WC_DataObject data)
        {
            return data.WorkGiverLabel;
        }

        protected override bool CreateStatusLabelText(WC_DataObject data) => false;

        protected override bool CreateStatusTipText(WC_DataObject data) => false;

        protected override string GetLabelText(WC_DataObject data, int index)
        {
            if (WC_DataContext.IsUniqueName(data))
            {
                return data.WorkGiverLabel;
            }
            else
            {
                return String.Format("{0}({1})", data.WorkGiverLabel, data.WorkGiverDefName);
            }
        }

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
