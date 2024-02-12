using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef Apparel_ArchotechSacrificerBelt;

        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}