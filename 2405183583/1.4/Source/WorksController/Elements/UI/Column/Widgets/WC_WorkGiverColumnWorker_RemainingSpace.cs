using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WorksController
{
    public class WC_WorkGiverColumnWorker_RemainingSpace : WC_WorkGiverColumnWorker
    {
        protected override float OptimalWidthFixed => -1000f;

        protected override TextAnchor HeaderTextAnchor => TextAnchor.MiddleCenter;

        protected override TextAnchor CellTextAnchor => TextAnchor.MiddleCenter;

        protected override float HeaderMarginLeft => 0f;

        protected override float ColumnMarginLeft => 0f;

        public override void DoCell(Rect rect, WC_DataObject data, WC_WorkGiverTable table)
        {
        }

        public override int GetMinWidth(WC_WorkGiverTable table)
        {
            return 0;
        }

        public override int GetMaxWidth(WC_WorkGiverTable table)
        {
            return 1000000;
        }

        public override int GetOptimalWidth(WC_WorkGiverTable table)
        {
            return this.GetMaxWidth(table);
        }

        public override int GetMinCellHeight(WC_DataObject data)
        {
            return 0;
        }
    }
}
