using System;
using System.Collections.Generic;
using System.Linq;

namespace Euphoric.YayosAmmunitionPatch
{
    public static class AmmoCalculation
    {
        public static GunAmmoSetting CalculateGunAmmoParameters(GunParameter parameter,
            Dictionary<AmmoType, float> ammoCostRatios, float maxAmmoSetting)
        {
            var averageAccuracy = AverageAccuracy(parameter);
            var armorPenetrationRating = AverageArmorPenetrationRating(parameter.ArmorPenetration);
            var explosionRating = AverageExplosionRating(parameter.ExplosionRadius);

            var leavesBehindAdditionaDamage = GetLeavesBehindDamage(parameter.LeavesBehind);
            
            var effectiveDamage = (parameter.BaseDamage+leavesBehindAdditionaDamage) * averageAccuracy * armorPenetrationRating * explosionRating;
            var damageToAmmoScale = GetDamageToAmmoScale(parameter.AmmoType) / ammoCostRatios[parameter.AmmoType];
            var ammoPerShot = (int)Math.Max(1, Math.Round(effectiveDamage * damageToAmmoScale));

            string warning = "";
            var maxAmmoPerShot = 70;
            if (ammoPerShot > maxAmmoPerShot)
            {
                warning += "Ammo count per charge clipped from " + ammoPerShot + " to " + maxAmmoPerShot +
                           ", due to pawn's inability to carry more than that." + Environment.NewLine;
                ammoPerShot = maxAmmoPerShot;
            }
            
            var shotsPerMinute = 60 / (parameter.Warmup + parameter.Cooldown + parameter.SecondsBetweenBurstShots * (parameter.Burst - 1)) * parameter.Burst;
            shotsPerMinute = Math.Round(shotsPerMinute, 2);

            var ammoPerMinute = shotsPerMinute * ammoPerShot;
            var gunShots = (int)Math.Round(shotsPerMinute * 1.5 * maxAmmoSetting);
            return new GunAmmoSetting(averageAccuracy, armorPenetrationRating, explosionRating, effectiveDamage, ammoPerShot, shotsPerMinute, ammoPerMinute, gunShots, warning);
        }

        private static double AverageExplosionRating(double explosionRadius)
        {
            return Math.Max(1, explosionRadius);
        }

        private static double GetDamageToAmmoScale(AmmoType ammoType)
        {
            switch (ammoType)
            {
                case AmmoType.Primitive:
                case AmmoType.PrimitiveFire:
                case AmmoType.PrimitiveSpecial:
                    return 1.0;
                case AmmoType.Industrial:
                case AmmoType.IndustrialFire:
                case AmmoType.IndustrialSpecial:
                    return 2.0;
                case AmmoType.Spacer:
                case AmmoType.SpacerFire:
                case AmmoType.SpacerSpecial:
                    return 3.0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ammoType), ammoType, null);
            }
        }

        public static double AverageArmorPenetrationRating(double armorPiercing)
        {
            if (armorPiercing <= 0.00)
            {
                return 0.45;
            }

            if (armorPiercing <= 0.36)
            {
                return (armorPiercing - 0.00) / (0.36 - 0.00) * (0.76 - 0.45) + 0.45;
            }

            if (armorPiercing <= 0.80)
            {
                return (armorPiercing - 0.36) / (0.80 - 0.36) * (0.925 - 0.76) + 0.76;
            }

            if (armorPiercing <= 1.30)
            {
                return (armorPiercing - 0.80) / (1.30 - 0.80) * (0.986 - 0.925) + 0.925;
            }
            
            if (armorPiercing <= 2.00)
            {
                return (armorPiercing - 1.30) / (2.00 - 1.30) * (1.00 - 0.986) + 0.986;
            }

            return 1;
        }

        public static double RangeAccuracyBias(double range)
        {
            if (range <= 3)
            {
                return 0;
            }
            
            if (range <= 12)
            {
                return (range - 3) / (12 - 3) * (1.0 - 0.75) + 0.75;
            }

            if (range <= 25)
            {
                return (range - 12) / (25 - 12) * (0.9 - 1.0) + 1.0;
            }

            if (range <= 38)
            {
                return (range - 25) / (38 - 25) * (0.4 - 0.9) + 0.9;
            }
            
            if (range <= 60)
            {
                return (range - 38) / (60 - 38) * (0 - 0.4) + 0.4;
            }

            return 0;
        }

        private static readonly Lazy<double> RangeAccuracyBiasTotal = new Lazy<double>(()=>Enumerable.Range(3,60).Select(rg=>RangeAccuracyBias(rg)).Sum());
        
        /// <summary>
        /// Calculates average accuracy of a weapon. With bias towards medium-range and against long-range.
        /// </summary>
        private static double AverageAccuracy(GunParameter parameter)
        {
            var averageAccuracy = Enumerable.Range(4, 60).Select(rg=>parameter.AccuracyAt(rg)*RangeAccuracyBias(rg)).Sum();
            var averageAccuracyForcedMiss = Enumerable.Range(4, 60).Select(rg=> parameter.ForcedAccuracyAt(rg)*RangeAccuracyBias(rg)).Sum();

            return Math.Max(averageAccuracy, averageAccuracyForcedMiss) / RangeAccuracyBiasTotal.Value;
        }
        
        private static double GetLeavesBehindDamage(LeavesBehind leavesBehind)
        {
            switch (leavesBehind)
            {
                case LeavesBehind.Nothing:
                    return 0;
                case LeavesBehind.Fuel:
                    return 30;
                case LeavesBehind.Smoke:
                    return 20;
                case LeavesBehind.Gas:
                    return 30;
                default:
                    throw new ArgumentOutOfRangeException(nameof(leavesBehind), leavesBehind, null);
            }
        }
    }
}