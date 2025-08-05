using RimWorld;
using Verse;
using System;

namespace BDsPlasmaWeapon
{
    public class StatPart_Quality : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing != null)
            {
                String QualityModifier;
                String preProcess = "QualityModifier".Translate();
                switch (req.QualityCategory)
                {
                    case QualityCategory.Awful:
                        QualityModifier = preProcess + "x50%";
                        break;
                    case QualityCategory.Poor:
                        QualityModifier = preProcess + "x80%";
                        break;
                    case QualityCategory.Good:
                        QualityModifier = preProcess + "+5%";
                        break;
                    case QualityCategory.Excellent:
                        QualityModifier = preProcess + "+10%";
                        break;
                    case QualityCategory.Masterwork:
                        QualityModifier = preProcess + "+15%";
                        break;
                    case QualityCategory.Legendary:
                        return "LegendaryQualityModfier".Translate() + "\n";
                    default:
                        QualityModifier = preProcess + "x100%";
                        break;
                }

                float hitPointPercentage = (float)req.Thing.HitPoints / (float)req.Thing.MaxHitPoints;
                float hitPointPenalty = 0;
                string hitPointModifier = "HealthAbove85NoPenalty".Translate();

                if (hitPointPercentage < 0.85)
                {
                    hitPointPenalty += (float)(0.85 - hitPointPercentage);
                    if (hitPointPercentage < 0.5)
                    {
                        hitPointPenalty += (float)(0.5 - hitPointPercentage) * 2;
                    }
                    hitPointModifier = "CurrentHitPointPercentage".Translate() + Math.Round(hitPointPercentage * 100, 0) + "%, -" + hitPointPenalty * 100 + "%" + "hitPointPenaltyApplied".Translate();
                }

                return QualityModifier + "\n" + hitPointModifier + "\n";
            }
            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing != null)
            {

                switch (req.QualityCategory)
                {
                    case QualityCategory.Awful:
                        val *= 0.5f;
                        break;
                    case QualityCategory.Poor:
                        val *= 0.8f;
                        break;
                    case QualityCategory.Good:
                        val += 0.05f;
                        break;
                    case QualityCategory.Excellent:
                        val += 0.1f;
                        break;
                    case QualityCategory.Masterwork:
                        val += 0.15f;
                        break;
                    case QualityCategory.Legendary:
                        val = 1f;
                        break;
                }

                float hitPointPercentage = (float)req.Thing.HitPoints / (float)req.Thing.MaxHitPoints;
                float hitPointPenalty = 0;
                if (hitPointPercentage < 0.85 && req.QualityCategory != QualityCategory.Legendary)
                {
                    hitPointPenalty += 1 - hitPointPercentage;
                    if (hitPointPercentage < 0.5)
                    {
                        hitPointPenalty += (float)(0.85 - hitPointPercentage) * 2;
                    }
                }
                val -= hitPointPenalty;
            }
        }
    }

}
