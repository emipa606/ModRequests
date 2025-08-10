using RimWorld;
using System.Text;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class StatPart_HemogenConcentration : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!TryGetHemogenConcentration(req, out float hemogenConcentration, /*explanation=*/out _))
                return;
            val *= hemogenConcentration;
        }

        public override string ExplanationPart(StatRequest req)
        {
            return TryGetHemogenConcentration(req, out _, out string explanation) ?
                explanation : null;
        }

        protected bool TryGetHemogenConcentration(StatRequest req, out float hemogenConcentration, out string explanation)
        {
            explanation = null;
            bool result = TryGetHemogenConcentrationImpl(req, out hemogenConcentration, out StringBuilder explanationBuilder);
            if (result)
                explanation = explanationBuilder?.ToString();
            return result;
        }

        // TODO: Add translation strings for each part of the explanation.
        protected bool TryGetHemogenConcentrationImpl(StatRequest req, out float hemogenConcentration, out StringBuilder explanationBuilder)
        {
            hemogenConcentration = 0f;
            explanationBuilder = new StringBuilder();
            if (!req.HasThing || !(req.Thing is Pawn pawn))
                return false;

            // For now, if a pawn does not have the hemogenic gene (that is, does not have the hemogen resource), the
            // pawn's hemogen concentration is always 1.
            if (!(pawn.genes?.GetGene(GeneDefOf.Hemogenic) is Gene_Hemogen hemogenGene))
            {
                hemogenConcentration = 1f;
                explanationBuilder.AppendLine($"Not hemogenic: x1");
                return true;
            }
            float hemogenValuePercent = hemogenGene.ValuePercent;

            float bloodPercent = 1f;
            Hediff bloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (!(bloodLossHediff is null))
            {
                bloodPercent -= bloodLossHediff.Severity;
            }

            if (bloodPercent <= 0f)
            {
                hemogenConcentration = 0f;
                explanationBuilder.AppendLine("Blood loss at or greater than 100%: =0.00.");
            }
            else
            {
                hemogenConcentration = hemogenValuePercent / bloodPercent;
                explanationBuilder.AppendLine($"Hemogen value / blood percent: {100f * hemogenValuePercent:F2} / {bloodPercent:P2}");
            }

            return true;
        }
    }
}
