using RimWorld;

namespace CensoredNudity
{
    public enum CensorBasis
    {
        [MayRequireIdeology]
        PawnIdeoligion,
        [MayRequireIdeology]
        PlayerIdeoligion,
        Never,
        Always,
        Covered,
        Uncovered
    }
}
