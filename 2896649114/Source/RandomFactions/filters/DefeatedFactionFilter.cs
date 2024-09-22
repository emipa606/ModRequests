using RimWorld;

namespace RandomFactions.filters
{
    public class DefeatedFactionFilter : FactionFilter
    {
        private bool isDefeated;
        public DefeatedFactionFilter(bool isDefeated)
        {
            this.isDefeated = isDefeated;
        }

        public override bool Matches(Faction f)
        {
            return f.defeated == this.isDefeated;
        }
    }
}
