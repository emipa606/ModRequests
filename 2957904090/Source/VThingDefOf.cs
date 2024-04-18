using RimWorld;
using Verse;

namespace zed_0xff.VNPE;

[DefOf]
public static class VThingDefOf {
    public static ThingDef VNPE_ConnectedBed;

    static VThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(VThingDefOf));
}

