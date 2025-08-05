using Verse;
using RimWorld;

namespace BDsPlasmaWeapon
{
    public class CompUseEffect_ReplacementAccelerator : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            ThingWithComps weapon = usedBy.equipment.Primary;
            weapon.HitPoints = (int)(weapon.MaxHitPoints * 0.75);
            parent.Destroy();
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            ThingWithComps weapon = p.equipment.Primary;
            if (weapon == null)
            {
                return "BDP_RepairFailNoWeapon".Translate();
            }
            if ((weapon.HitPoints / (float)weapon.MaxHitPoints) > 0.75)
            {
                return "BDP_RepairFailHitpoint".Translate();
            }
            if (weapon.TryGetComp<CompCasingReturn>() == null)
            {
                return "BDP_RepairFailWrongWeapon".Translate();
            }
            return base.CanBeUsedBy(p);
        }
    }
}
