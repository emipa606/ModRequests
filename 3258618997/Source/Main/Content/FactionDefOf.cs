using System;
using RimWorld;

namespace Necro
{
    
    [DefOf]
    public static class FactionDefOf
    {
        
        static FactionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FactionDefOf));
        }

        
        public static FactionDef Necronoid;

        
        public static FactionDef PlayerColony;
    }
}
