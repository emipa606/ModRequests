using RimWorld;
using Verse;


namespace BillDoorsFramework
{
    public class StatPart_BurstCountSecondaryVerb : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            CompSecondaryAmmo compVerb = req.Thing?.TryGetComp<CompSecondaryAmmo>();
            if (compVerb != null)
            {
                val = compVerb.Props.secondaryVerb.burstShotCount;
            }
        }
    }
}
