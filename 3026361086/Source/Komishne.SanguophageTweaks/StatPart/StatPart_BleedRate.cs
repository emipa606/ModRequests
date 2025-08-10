using RimWorld;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class StatPart_BleedRate : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            float bleedRate;
            if (!TryGetBleedRate(req, out bleedRate))
                return;
            val *= bleedRate;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return TryGetBleedRate(req, out float bleedRate) ?
                $"{"KOM.SanguophageTweaks.StatsReport_BleedRate".Translate()}: x{bleedRate:P2}\n" :
                null;
        }

        private bool TryGetBleedRate(StatRequest req, out float bleedRate)
        {
            bleedRate = 0f;
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return false;

            bleedRate = pawn.health?.hediffSet?.BleedRateTotal ?? 0f;
            return true;
        }
    }
}
