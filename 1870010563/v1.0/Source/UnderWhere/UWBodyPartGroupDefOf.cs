using System.Reflection;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Harmony;

namespace UnderWhere
{ 
    [DefOf]
    public static class UWBodyPartGroupDefOf
    {
        static UWBodyPartGroupDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(UWBodyPartGroupDefOf));
        }

        // Token: 0x04002142 RID: 8514
        public static BodyPartGroupDef Waist;
    }
}
