using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WorksController
{
    public class WC_WorkGiverTableDef : Def
    {
        public List<WC_WorkGiverColumnDef> columns;

        public Type workerClass = typeof(WC_WorkGiverTable);

        public int minWidth = 1148;

    }
}
