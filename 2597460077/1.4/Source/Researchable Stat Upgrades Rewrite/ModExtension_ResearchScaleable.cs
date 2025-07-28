using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ResearchableStatUpgrades
{
    public abstract class ModExtension_ResearchScaleable : DefModExtension
    {
        public abstract void ResolveCost(ref ResearchProjectDef def, int factor);
    }
}
