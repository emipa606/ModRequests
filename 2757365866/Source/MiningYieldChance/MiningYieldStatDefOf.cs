using RimWorld;

namespace MiningYieldChance
{
    [DefOf]
    public static class MiningYieldStatDefOf
    {
        static MiningYieldStatDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MiningYieldStatDefOf));

        public static StatDef MiningChunkDropChance;
    }
}
