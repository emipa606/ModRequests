using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RandomFactions.filters
{
    public class TechLevelFactionDefFilter : FactionDefFilter
    {
        private TechLevel minTechLevel;
        private TechLevel maxTechLevel;
        /* Tech levels in order:
        Undefined,
        Animal,
        Neolithic,
        Medieval,
        Industrial,
        Spacer,
        Ultra,
        Archotech
        */
        public TechLevelFactionDefFilter(TechLevel minTL, TechLevel maxTL)
        {
            this.minTechLevel = minTL;
            this.maxTechLevel = maxTL;
        }

        public override bool Matches(FactionDef def)
        {
            int t = toInt(def.techLevel);
            int low = toInt(minTechLevel);
            int high = toInt(maxTechLevel);
            return t >= low && t <= high;
        }

        private int toInt(TechLevel tl)
        {
            return (int)tl;
        }
    }
}
