using RimWorld;
using System.Collections.Generic;

namespace RandomFactions.filters
{
    public class FactionDefNoOpFilter : FactionDefFilter
    {

        public FactionDefNoOpFilter()
        {
            // do nothing
        }

        public override bool Matches(FactionDef f)
        {
            // do nothing
            return true;
        }
    }
}
