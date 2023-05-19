using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WorksController
{

    [DefOf]
    public class WC_WorkGiverTableDefOf
    {
        public static WC_WorkGiverTableDef WC_Configure;
    }

    public class ExcludeTypeDef : Def
    {
        public List<string> excludeTypes;
    }
}
