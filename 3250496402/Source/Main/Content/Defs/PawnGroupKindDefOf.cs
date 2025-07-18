using RimWorld;

namespace Kraltech_Industries;

[DefOf]
public static class PawnGroupKindDefOf
{
    public static PawnGroupKindDef MechVariantInvaders;

    public static PawnGroupKindDef Combat;

    static PawnGroupKindDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnGroupKindDefOf));
    }
}