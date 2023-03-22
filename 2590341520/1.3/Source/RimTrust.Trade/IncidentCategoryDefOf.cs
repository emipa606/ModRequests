using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimTrust.Trade
{
    [DefOf]
    public static class IncidentCategoryDefOf
    {
        static IncidentCategoryDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentCategoryDefOf));
        }

        public static IncidentCategoryDef Special;
    }
}
