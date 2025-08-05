using Verse;
using RimWorld;

namespace BDsPlasmaWeapon
{
    public class CompUseEffect_LizionPopper : CompUseEffect
    {
        CompProximityLizionPopper compProximityLizionPopper;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compProximityLizionPopper = parent.TryGetComp<CompProximityLizionPopper>();
        }

        public override void DoEffect(Pawn usedBy)
        {
            compProximityLizionPopper.trigger();
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (compProximityLizionPopper == null || !compProximityLizionPopper.isAvailableForPop)
            {
                return "unavailable".Translate();
            }
            return base.CanBeUsedBy(p);
        }
    }
}
