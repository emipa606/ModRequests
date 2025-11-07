using RimWorld;
using Verse;
namespace BillDoorsFramework
{
    public class StatPart_SecondaryMagazineCapacity : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing)
            {
                return;
            }

            Thing thing = req.Thing;
            CompSecondaryAmmo CompAmmo = thing.TryGetComp<CompSecondaryAmmo>();

            if (CompAmmo != null && !CompAmmo.IsSharedAmmo && CompAmmo.IsSecondaryAmmoSelected)
            {
                val = CompAmmo.Props.secondaryAmmoProps.magazineSize;
            }
        }
    }
}
