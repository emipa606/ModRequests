using RimWorld;
using UnityEngine;
using Verse;

namespace ArmoredUp
{
    public class StatPart_BluntAdjustment : StatPart
    {
        private float bluntBonus = 0f;
        public static float GetBluntArmorBonus(Thing armorThing, float blunt)
        {
            float sharp = armorThing.GetStatValue(StatDefOf.ArmorRating_Sharp);
            //Shortcircuit if blunt is higher, don't need to touch it.
            if (blunt > sharp || blunt <= 0f || sharp <= 0f)
                return 0f;
            //Tl;DR for default settings: double the positive relative difference between our blunt:sharp ratio and that of 
            //  the flak vest, which is our prototypical 'soft armor'
            float ratio = blunt / sharp;
            float ratioDiff = ratio - ArmoredUp.settings.HardArmorRatio;
            if (ratioDiff > 0f || ArmoredUp.settings.SoftArmorPenaltyActive)
                return sharp * Mathf.Min(ratioDiff * (ArmoredUp.settings.HardArmorBonusMult - 1f), 1f - ratio);
            return 0f;


        }
        
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                bluntBonus = GetBluntArmorBonus(req.Thing, val);
                val += bluntBonus;
                //val += 1f;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && bluntBonus > 0.0001f)
            {
                //return "Armored Up StatPart_BluntAdjustment: " + 1f.ToStringPercent();
                return "'Hard Armor' Bonus: +" + bluntBonus.ToStringPercent();
            }
            return null;
        }
    }
}