using RimWorld;
using Verse;

namespace LifeSupport {
    public class Hediff_LifeSupport : HediffWithComps {
        public override bool ShouldRemove => pawn.CurrentBed() == null;

        public override bool Visible => true;
    }
}
