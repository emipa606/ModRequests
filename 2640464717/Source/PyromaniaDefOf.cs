using RimWorld;

namespace TorchesArentRecRoomsForBetterPyromania
{
    [DefOf]
    public static class PyromaniaDefOf
    {
        public static JoyGiverDef Pyromaniac_WatchFlame;

        static PyromaniaDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PyromaniaDefOf));
        }
    }
}
