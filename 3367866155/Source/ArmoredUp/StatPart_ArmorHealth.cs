using RimWorld;
using Verse;

namespace ArmoredUp
{
    public class StatPart_ArmorHealth : StatPart
    {
        public static float GetDamageFactorForArmor(Thing armorThing)
        {
            return ArmoredUp.settings.MinDamagedArmorEffectiveness +
                   (1f - ArmoredUp.settings.MinDamagedArmorEffectiveness) * armorThing.HitPoints /
                   armorThing.MaxHitPoints;
        }

        public static float GetDamageFactorForBattered(Pawn pawn)
        {
            return ArmoredUp.settings.MinDamagedArmorEffectiveness +
                (1f - ArmoredUp.settings.MinDamagedArmorEffectiveness) * 
                (1f - (pawn.health.hediffSet.GetFirstHediffOfDef(ArmoredUpDefOf.BatteredArmor)?.Severity ?? 0f));
        }
        
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing.def.useHitPoints)
            {
                val *= GetDamageFactorForArmor(req.Thing);
            }
            else if (req.HasThing && !req.Thing.def.useHitPoints)
            {
                if (req.Thing is Pawn pawn)
                    val *= GetDamageFactorForBattered(pawn);
                else if (req.Thing is Apparel apparel)
                    val *= GetDamageFactorForBattered(apparel.Wearer);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing.def.useHitPoints)
            {
                return $"Armor Durability: x{GetDamageFactorForArmor(req.Thing):P} \n" +
                       $"(Minimum Effective Rating: {ArmoredUp.settings.MinDamagedArmorEffectiveness * req.Thing.GetStatValue(parentStat):P})";
                
            }
            else if (req.HasThing && !req.Thing.def.useHitPoints)
            {
                if (req.Thing is Pawn pawn)
                    return $"Battered Effect: x{GetDamageFactorForBattered(pawn):P} \n" + 
                           $"(Minimum Effective Rating: {ArmoredUp.settings.MinDamagedArmorEffectiveness * req.Thing.GetStatValue(parentStat):P})";
                else if (req.Thing is Apparel apparel)
                    return $"Battered Effect: x{GetDamageFactorForBattered(apparel.Wearer):P} \n" + 
                           $"(Minimum Effective Rating: {ArmoredUp.settings.MinDamagedArmorEffectiveness * req.Thing.GetStatValue(parentStat):P})";
            }
            return null;
        }
    }
}