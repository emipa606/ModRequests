using RimWorld;

namespace RandomFactions.filters
{
    public class OneOnlyFactionDefFilter : FactionDefFilter
    {
        private bool isOneOnly;
        public OneOnlyFactionDefFilter(bool isOneOnly)
        {
            this.isOneOnly = isOneOnly;
        }

        public override bool Matches(FactionDef def)
        {
            return (def.maxCountAtGameStart == 1) == this.isOneOnly;
        }
    }
}
