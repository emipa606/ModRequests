using System;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Message2Letter
{
    // This class is simply a stub to make error go away
    // thanks to bug report from user that shows that
    // if a class lack a workerClass error will be thrown, not sure
    // under what circumstance.
    public class E1D8_IncidentWorker_Stub : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return false;
        }
    }
}
