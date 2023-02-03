using RimWorld;

namespace ImprovedInsectoids
{
    [DefOf]
    public static class MemeDefOf
    {
        [MayRequireIdeology]
        public static MemeDef InsectoidPrimacy;

        static MemeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MemeDefOf));
        }
    }
}
