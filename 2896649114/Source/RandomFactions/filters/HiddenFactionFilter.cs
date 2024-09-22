using RimWorld;

namespace RandomFactions.filters
{
    public class HiddenFactionFilter : FactionFilter
    {
        private bool isHidden;
        public HiddenFactionFilter(bool isHidden)
        {
            this.isHidden = isHidden;
        }

        public override bool Matches(Faction f)
        {
            return f.Hidden == this.isHidden;
        }
    }
}
