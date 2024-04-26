using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    [DefOf]
    public static class RulePackDefOf_AE
    {
        static RulePackDefOf_AE()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RulePackDefOf_AE));
        }

        public static RulePackDef DamageEvent_Toxic_AE;

        public static RulePackDef DamageEvent_Corrosive_AE;

    }
}
