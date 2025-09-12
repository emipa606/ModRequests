using Verse;

namespace Necro;

public class VerbSpewFireReloadable : Verb_SpewFire
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
