using RimWorld;
using Verse;

namespace zed_0xff.LoftBed
{
    [DefOf]
    public static class VThingDefOf
    {
        public static ThingDef LoftBed;

        static VThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(VThingDefOf));
    }
}
