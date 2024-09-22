using RimWorld;
using System.Collections.Generic;

namespace RandomFactions.filters
{
    public class FactionDefNameFilter : FactionDefFilter
    {
        private HashSet<string> names;
        bool include;

        public FactionDefNameFilter(params string[] names)
        {
            this.names = new HashSet<string>();
            foreach (var n in names)
            {
                this.names.Add(n);
            }
            include = true;
        }
        public FactionDefNameFilter(bool include, params string[] names)
        {
            this.names = new HashSet<string>();
            foreach (var n in names)
            {
                this.names.Add(n);
            }
            this.include = include;
        }

        public override bool Matches(FactionDef f)
        {
            return include == names.Contains(f.defName);
        }
    }
}
