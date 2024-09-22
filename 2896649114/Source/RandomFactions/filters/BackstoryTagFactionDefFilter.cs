using RimWorld;
using Verse;

namespace RandomFactions.filters
{
    public class BackstoryTagFactionDefFilter : FactionDefFilter
    {
        private BackstoryCategoryFilter tag; // eg "Offworld" or "Pirate"
        public BackstoryTagFactionDefFilter(BackstoryCategoryFilter tag)
        {
            this.tag = tag;
        }

        public override bool Matches(FactionDef f)
        {
            foreach(var tag in f.backstoryFilters)
            {
                foreach(var cat in tag.categories) {
                    if (cat.Equals(this.tag)){
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
