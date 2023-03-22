using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimTrust.Trade
{
    [DefOf]
    public static class ThoughtDefOf
    {
        static ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
        }

        public static ThoughtDef Ascension;
    }
}
