using Kraltech_Industries;
using Verse;

namespace Kraltech_Industries;

public class Verb_Shoot_AccurateShotReloadable : Verb_Shoot_AccurateShot
{
    public override bool Available()
    {
        var charges = EquipmentSource?.GetComp<CompReloadable_Weapon>()?.RemainingCharges;
        if (charges.HasValue)
            return charges.Value > 0 && base.Available();
        return base.Available();
    }

    protected override bool TryCastShot()
    {
        if (!base.TryCastShot()) return false;
        EquipmentSource?.GetComp<CompReloadable_Weapon>()?.NotifyShot();
        return true;
    }
}
