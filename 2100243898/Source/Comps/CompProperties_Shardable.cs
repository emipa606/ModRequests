using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTechprinting.Comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace DTechprinting.Comps
{
    public class CompProperties_Shardable : CompProperties
    {
        public CompProperties_Shardable()
        {
            this.compClass = typeof(CompShardable);
        }
    }
}
