using Verse;
using RimWorld;
using System.Collections.Generic;

namespace HumanResources
{
    public class CompProperties_Knowledge : CompProperties
    {
        public List<ResearchProjectDef> innateKnownTechs = new List<ResearchProjectDef>();
        public List<ThingDef> innateKnownWeapons = new List<ThingDef>();

        public CompProperties_Knowledge()
        {
            compClass = typeof(CompKnowledge);
        }
    }
}
