using RimWorld;
using Verse;

namespace Kraltech_Industries;

[DefOf]
public static class PawnKindDefOf
{
    public static PawnKindDef Mech_Dragon;

    public static PawnKindDef Mech_Valkyrian;

    public static PawnKindDef Mech_AlphaCentipede;

    public static PawnKindDef Mech_AlphaLancer;

    public static PawnKindDef Mech_AlphaScyther;

    public static PawnKindDef Mech_AlphaPikeman;

    static PawnKindDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }
}