using Verse;

namespace DRNE_NeutroamineExpansion
{
    public class ModExt_NeutrogenesisXMLSettings : DefModExtension
    {
        public PawnKindDef primaryPawnKind;

        public PawnKindDef secondaryPawnKind = null;

        public PawnKindDef ternaryPawnKind = null;

        public float requiredSizeForPrimary = 1f;

        public float requiredSizeForSecondary = 0.5f;

        public float requiredSizeForTernary = 0f;

        public float ChangedBioAge = 1f;
    }
}