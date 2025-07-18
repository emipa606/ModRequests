using RimWorld;

namespace Kraltech_Industries;

[DefOf]
public static class ThoughtDefOf
{
    public static ThoughtDef ObservedSupremeApocritonTrophy;

    static ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
    }
}