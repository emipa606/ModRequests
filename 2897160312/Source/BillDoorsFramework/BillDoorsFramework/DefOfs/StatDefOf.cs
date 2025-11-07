using RimWorld;

namespace BillDoorsFramework
{
    [DefOf]
    public static class StatDefOf
    {
        static StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
        }
        public static StatDef BDsKeyCardRequirmentStat;

        public static StatDef ArmorRating_Sharp;
    }
}
