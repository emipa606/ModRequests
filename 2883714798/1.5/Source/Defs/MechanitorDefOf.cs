using RimWorld;

namespace MechPsyControlRange_Hydroxyapatite
{
    [DefOf]
    public static class MechanitorDefOf
    {
        public static StatDef Hydroxyapatite_MechCommandDistance;
        static MechanitorDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MechanitorDefOf));
        }
    }
}
