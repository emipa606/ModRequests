using Reload.Components;
using Reload.Defs;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Reload
{
    public class AmmoUtils
    {
        static Dictionary<string, int> _ammoQuantity = new Dictionary<string, int>(); // {weapon def, quantity}

        static List<DamageDef> _explosiveType = new List<DamageDef>
        {
           DamageDefOf.Bomb,
           DamageDefOf.Burn,
           DamageDefOf.Flame,
           DamageDefOf.Smoke,
           DamageDefOf.ToxGas,
           DamageDefOf.Vaporize
        };

        static List<DamageDef> _specialType = new List<DamageDef>
        {
            DamageDefOf.Deterioration,
            DamageDefOf.EMP,
            DamageDefOf.Extinguish,
            DamageDefOf.Frostbite,
            DamageDefOf.Smoke,
            DamageDefOf.Stun,
            DamageDefOf.Rotting,
            DamageDefOf.TornadoScratch
        };

        public static AmmoDef GetProperAmmo(ThingDef def)
        {
            TechLevel techLevel = def.techLevel;
            ProjectileProperties projectile = def.Verbs[0]?.defaultProjectile?.projectile;
            string defName;

            if (projectile == null)
            {
                defName = techLevel <= TechLevel.Medieval ? "R_Ammo_Primitive" :
                    techLevel <= TechLevel.Industrial ? "R_Ammo_Industrial" : "R_Ammo_Spacer";

                return (AmmoDef)ThingDef.Named(defName);
            }

            AmmoType ammoType = AmmoType.Normal;
            if (_explosiveType.Contains(projectile.damageDef))
                ammoType = AmmoType.Explosive;
            else if (_specialType.Contains(projectile.damageDef))
                ammoType = AmmoType.Special;

            if(techLevel <= TechLevel.Medieval)
            {
                if (ammoType == AmmoType.Normal)
                    defName = "R_Ammo_Primitive";
                else if (ammoType == AmmoType.Explosive)
                    defName = "R_Ammo_Primitive_Explosive";
                else
                    defName = "R_Ammo_Primitive_Special";
            }
            else if (techLevel <= TechLevel.Industrial)
            {
                if (ammoType == AmmoType.Normal)
                    defName = "R_Ammo_Industrial";
                else if (ammoType == AmmoType.Explosive)
                    defName = "R_Ammo_Industrial_Explosive";
                else
                    defName = "R_Ammo_Industrial_Special";
            }
            else
            {
                if (ammoType == AmmoType.Normal)
                    defName = "R_Ammo_Spacer";
                else if (ammoType == AmmoType.Explosive)
                    defName = "R_Ammo_Spacer_Explosive";
                else
                    defName = "R_Ammo_Spacer_Special";
            }

            return (AmmoDef)ThingDef.Named(defName);
        }
        public static int GetAmmoCapacity(CompReload comp)
        {
            if (comp == null)
                return 0;
            if (_ammoQuantity.ContainsKey(comp.parent.def.defName))
                return _ammoQuantity[comp.parent.def.defName];

            VerbProperties verb = comp.VerbProperties[0];
            int warmupTick = (int)(verb.warmupTime * 60f);
            int coolDownTick = (int)(comp.parent.GetStatValue(StatDefOf.RangedWeapon_Cooldown) * 60f);

            float time = ((warmupTick + coolDownTick) * (comp.MagazineCapacity / verb.burstShotCount) + (verb.burstShotCount - 1) * verb.ticksBetweenBurstShots) / 60f;
            time = Math.Max(0.0001f, time);
            int amount = (int)((float)comp.MagazineCapacity * Math.Round(30f / time));

            _ammoQuantity[comp.parent.def.defName] = amount;
            return amount;
        }
    }
}
