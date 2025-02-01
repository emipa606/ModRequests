using RimWorld;

namespace LostTechnology
{
    public class IncidentWorker_NewTechprint : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            return LostTechnologySettings.endeavouringSystemEnabled;
        }
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!LostTechnologySettings.endeavouringSystemEnabled)
            {
                return false;   
            }
            foreach (var site in Core.MakeTechprintSites())
            {
                SendStandardLetter(parms, site);
            }
            return true;
        }
    }
}
