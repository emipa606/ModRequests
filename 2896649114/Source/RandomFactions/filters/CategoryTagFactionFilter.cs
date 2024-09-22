using RimWorld;
using Verse;

namespace RandomFactions.filters
{
    public class CategoryTagFactionFilter : FactionFilter
    {
        private string tag; // eg "Outlander"
        public CategoryTagFactionFilter(string tag)
        {
            this.tag = tag;
        }

        public override bool Matches(Faction f)
        {
            return f.def.categoryTag.EqualsIgnoreCase(this.tag);
        }
    }
}
