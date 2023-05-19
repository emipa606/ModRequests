using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_StatusLabel : WC_WorkGiverColumnWorker_Label
    {

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override bool IsStatusColumn => true;

        protected override string CompareValue(WC_DataObject data)
        {
            if (!data.Active)
            {
                return new string(new[] { '\uffff' });
            }
            return GetLabelText(data, 0);
        }

        protected override bool CreateStatusLabelText(WC_DataObject data) => WC_DataContext.CreateStatusLabelText(data);

        protected override bool CreateStatusTipText(WC_DataObject data) => WC_DataContext.CreateStatusTipText(data);
 
        protected override string GetLabelText(WC_DataObject data, int index) => WC_DataContext.GetStatusLabelText(data.WorkGiverDefName);

        protected override string GetTipText(WC_DataObject data, int index) => WC_DataContext.GetStatusTipText(data.WorkGiverDefName);

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
