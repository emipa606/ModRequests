using RimWorld;
using Verse;

namespace RandomFactions.filters
{
    public class CategoryTagFactionDefFilter : FactionDefFilter
    {
        private string tag; // eg "Outlander"
        public CategoryTagFactionDefFilter(string tag)
        {
            this.tag = tag;
        }

        public override bool Matches(FactionDef f)
        {
            return f.categoryTag.EqualsIgnoreCase(this.tag);
        }
    }
}
