using RimWorld;
using Verse;

namespace Hydroxyapatite_AdvancedBandNodes
{
    [DefOf]
    public class AdvancedBandNodeDefOf
    {
        [MayRequireBiotech]
        public static ThingDef Hydroxyapatite_AdvancedBandNode;
        static AdvancedBandNodeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AdvancedBandNodeDefOf));
        }
    }
}
