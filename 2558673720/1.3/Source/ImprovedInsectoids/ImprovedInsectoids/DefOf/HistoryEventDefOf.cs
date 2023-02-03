using RimWorld;

namespace ImprovedInsectoids
{
    [DefOf]
    public static class HistoryEventDefOf
    {
        [MayRequireIdeology]
        public static HistoryEventDef KilledInnocentInsectoid;

        [MayRequireIdeology]
        public static HistoryEventDef SlaughteredInsectoid;

        static HistoryEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HistoryEventDefOf));
        }
    }
}
