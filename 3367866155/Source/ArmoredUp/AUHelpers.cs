using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmoredUp
{
    [StaticConstructorOnStartup]
    public static class ArmorUtils
    {
        
        public static bool IsPrimitiveVsGuns(Thing armorThing, DamageDef damageDef, DamageInfo dinfo, bool isWornArmor)
        {
            return isWornArmor && dinfo.Weapon != null && damageDef.isRanged && (int)armorThing.def.techLevel <= 3 &&
                   (int)dinfo.Weapon.techLevel >= 4;
        }

        public static void SetMetalArmor(Thing armorThing, Pawn pawn, out bool metalArmor, bool isWornArmor)
        {
            if (isWornArmor)
            {
                metalArmor = armorThing.def.apparel.useDeflectMetalEffect || (armorThing.Stuff != null && armorThing.Stuff.IsMetal);
            }
            else
            {
                metalArmor = pawn.RaceProps.IsMechanoid;
            }
        }

        public static float GetModifiedBluntArmor(Thing armorThing)
        {
            float sharp = armorThing.GetStatValue(StatDefOf.ArmorRating_Sharp);
            float blunt = armorThing.GetStatValue(StatDefOf.ArmorRating_Blunt);
            //Shortcircuit if blunt is higher, don't need to touch it.
            if (blunt > sharp)
                return blunt;
            //Tl;DR for default settings: double the positive relative difference between our blunt:sharp ratio and that of 
            //  the flak vest, which is our prototypical 'soft armor'
            float ratioDiff = blunt / sharp - ArmoredUp.settings.HardArmorRatio;
            if (ratioDiff > 0f || ArmoredUp.settings.SoftArmorPenaltyActive)
                return sharp * Mathf.Clamp01(ratioDiff * ArmoredUp.settings.HardArmorBonusMult + ArmoredUp.settings.HardArmorRatio);
            else
                return blunt;


        }

        public static DamageInfo BluntTraumatizeTheDInfo(DamageInfo dinfo, float amount, float AP)
        {
            //DamageInfo bluntTraumaDamageInfo = new DamageInfo(dinfo);
            DamageInfo bluntTraumaDamageInfo = new DamageInfo(DamageDefOf.Blunt, amount, AP, dinfo.Angle, dinfo.Instigator, GetSurfacePart(dinfo.HitPart), dinfo.Weapon, DamageInfo.SourceCategory.ThingOrUnknown, null, dinfo.InstigatorGuilty);
            bluntTraumaDamageInfo.SetBodyRegion(dinfo.Height, dinfo.Depth);
            bluntTraumaDamageInfo.SetWeaponBodyPartGroup(dinfo.WeaponBodyPartGroup);
            bluntTraumaDamageInfo.SetWeaponHediff(dinfo.WeaponLinkedHediff);
            bluntTraumaDamageInfo.SetInstantPermanentInjury(dinfo.InstantPermanentInjury);
            bluntTraumaDamageInfo.SetAllowDamagePropagation(dinfo.AllowDamagePropagation);
            bluntTraumaDamageInfo.SetIgnoreArmor(true);
            return bluntTraumaDamageInfo;
        }
        

        public static void BluntIfSharp(this ref DamageInfo dinfo)
        {
            if (dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp)
            {
                dinfo.Def = DamageDefOf.Blunt;
            }
        }

        public static BodyPartRecord GetSurfacePart(BodyPartRecord bodyPart)
        {
            BodyPartRecord bodyPartRecord = bodyPart;
            if (bodyPartRecord != null)
            {
                while (bodyPartRecord.parent != null && bodyPartRecord.depth != BodyPartDepth.Outside)
                {
                    bodyPartRecord = bodyPartRecord.parent;
                }
            }

            return bodyPartRecord;
        }

        public static bool IsBluntTraumaSouce(DamageDef damageDef, StatDef armorRatingStat)
        {
            return armorRatingStat == StatDefOf.ArmorRating_Sharp ||
                   (ArmoredUp.settings.HeatBluntTraumaActive &&
                    damageDef.workerClass != typeof(DamageWorker_Flame) &&
                    armorRatingStat == StatDefOf.ArmorRating_Heat);
        }

        public static float GetStoppingPower(DamageDef damageDef, DamageInfo dinfo)
        {
            return damageDef.isRanged 
                ? Mathf.Min(ArmoredUp.settings.MaxStoppingPower,
                    dinfo.Weapon?.Verbs?.FirstOrDefault()?.defaultProjectile?.projectile?.stoppingPower 
                    ?? dinfo.Def.defaultStoppingPower)
                : ArmoredUp.settings.MeleeArtificialStoppingPower;
        }
    }
}