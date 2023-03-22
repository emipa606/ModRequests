using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace RimTrust.Trade
{
    [DefOf]
    public static class IncidentDefOf
    {
        static IncidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }

        public static IncidentDef InterestDrop;
        public static IncidentDef TrusteeCollector;
        public static IncidentDef NeuralSync;
    }
}

