using RimWorld;
using System.Collections.Generic;
using Verse;

namespace NoobertNebulous
{
    public class StorytellerCompProperties_NoobertNebulous : StorytellerCompProperties
    {
        public List<IncidentDef> goodIncidents;

        public StorytellerCompProperties_NoobertNebulous()
        {
            compClass = typeof(StorytellerComp_NoobertNebulous);
        }
    }

    public class StorytellerComp_NoobertNebulous : StorytellerComp
    {
        public StorytellerCompProperties_NoobertNebulous Props => props as StorytellerCompProperties_NoobertNebulous;
        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            yield break;
        }

        public FiringIncident TryGetIncident(IncidentCategoryDef category)
        {
            IncidentParms parms = GenerateParms(category, Find.RandomPlayerHomeMap);
            if (TrySelectRandomIncident(UsableIncidentsInCategory(category, parms), out var foundDef))
            {
                return new FiringIncident(foundDef, this, parms);
            }
            return null;
        }

        public FiringIncident TryGetIncident()
        {
            foreach (var goodIncident in Props.goodIncidents.InRandomOrder())
            {
                IncidentParms parms = GenerateParms(goodIncident.category, Find.RandomPlayerHomeMap);
                if (goodIncident.Worker.CanFireNow(parms))
                {
                    return new FiringIncident(goodIncident, this, parms);
                }
            }
            return null;
        }
    }
}
