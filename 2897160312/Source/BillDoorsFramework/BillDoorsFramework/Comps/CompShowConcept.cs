using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class CompShowConcept : ThingComp
    {
        public CompProperties_ShowConcept Props
        {
            get { return (CompProperties_ShowConcept)this.props; }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.Props.concepts == null) return;
            foreach (var concept in this.Props.concepts)
            {
                LessonAutoActivator.TeachOpportunity(concept, OpportunityType.GoodToKnow);
            }
        }
    }

    public class CompProperties_ShowConcept : CompProperties
    {
        public CompProperties_ShowConcept()
        {
            this.compClass = typeof(CompShowConcept);
        }

        public List<ConceptDef> concepts;
    }
}
