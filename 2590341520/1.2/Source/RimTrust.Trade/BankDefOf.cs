using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    [DefOf]
    public static class BankDefOf
    {
        public static ThingDef BankNote;

        static BankDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BankDefOf));
        }
    }
}