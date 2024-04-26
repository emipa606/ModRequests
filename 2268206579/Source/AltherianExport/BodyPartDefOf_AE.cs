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
    public class BodyPartDefOf_AE 
    {
        static BodyPartDefOf_AE()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BodyPartDefOf_AE));
        }

        public static BodyPartDef Ear; // so we can access the ears to apply a hediff to them



    }
}
