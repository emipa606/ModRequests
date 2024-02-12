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
    public static class ThoughtDefOf
    {
        public static ThoughtDef KnowColonistSacrificed;
        public static ThoughtDef KnowPrisonerSacrificed;
        public static ThoughtDef KnowAnimalSacrificed;

        public static ThoughtDef WorryFree;

            static ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
        }
    }
}