using System;
using Verse;

namespace UnderWhere
{
    public class CompMaleOnly : ThingComp
    {
        public CompMaleOnlyProperties Props => (CompMaleOnlyProperties)this.props;
    }
    public class CompMaleOnlyProperties : CompProperties
    {
        public CompMaleOnlyProperties()
        {
            this.compClass = typeof(CompMaleOnly);
        }
        public CompMaleOnlyProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
    public class CompFemaleOnly : ThingComp
    {
        public CompFemaleOnlyProperties Props => (CompFemaleOnlyProperties)this.props;
    }
    public class CompFemaleOnlyProperties : CompProperties
    {
        public CompFemaleOnlyProperties()
        {
            this.compClass = typeof(CompFemaleOnly);
        }
        public CompFemaleOnlyProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
