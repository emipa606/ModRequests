using RimWorld;

namespace Hospitality
{
    /// <summary>
    /// This class is for debugging purposes
    /// </summary>
    public class IncidentWorker_VisitorGroupMax : IncidentWorker_VisitorGroup
    {
        protected override int GetGroupSize()
        {
            return ModSettings_Hospitality.maxGuestGroupSize;
        }

        public override bool CanFireNowSub(IncidentParms parms)
        {
            return false;
        }
    }
}