using RimWorld;
using Verse;

namespace RandomFactions.filters
{
    public class BackstoryTagFactionFilter : FactionFilter
    {
        private BackstoryCategoryFilter tag; // eg "Offworld" or "Pirate"
        public BackstoryTagFactionFilter(BackstoryCategoryFilter tag)
        {
            this.tag = tag;
        }

        public override bool Matches(Faction f)
        {
            foreach(var tag in f.def.backstoryFilters)
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
