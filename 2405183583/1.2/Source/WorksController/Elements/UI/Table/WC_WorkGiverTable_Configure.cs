using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WorksController
{
    public class WC_WorkGiverTable_Configure : WC_WorkGiverTable
    {
        public WC_WorkGiverTable_Configure(WC_WorkGiverTableDef def, Func<IEnumerable<WC_DataObject>> datasGetter, int uiWidth, int uiHeight) : base(def, datasGetter, uiWidth, uiHeight)
        {
        }
    }
}
