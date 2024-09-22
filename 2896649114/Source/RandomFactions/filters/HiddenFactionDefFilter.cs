using RimWorld;

namespace RandomFactions.filters
{
    public class HiddenFactionDefFilter : FactionDefFilter
    {
        private bool isHidden;
        public HiddenFactionDefFilter(bool isHidden)
        {
            this.isHidden = isHidden;
        }

        public override bool Matches(FactionDef f)
        {
            return f.hidden == this.isHidden;
        }
    }
}
