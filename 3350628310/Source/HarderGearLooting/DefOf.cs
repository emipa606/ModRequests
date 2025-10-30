using RimWorld;
using Verse;

namespace HarderGearLooting
{
    [DefOf]
    public static class HardGearLooting_DefOf
    {
        public static JobDef UseBiocodeReformatter;
        public static JobDef ApplyBiocodeReformatter;
        public static HediffDef Acidifier;

        static HardGearLooting_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(HardGearLooting_DefOf));
    }
}
