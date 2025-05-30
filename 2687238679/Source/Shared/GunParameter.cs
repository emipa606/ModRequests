using System;

namespace Euphoric.YayosAmmunitionPatch
{
    public class GunParameter
    {
        public GunParameter(string name, AmmoType ammoType, double warmup, double cooldown,
            double secondsBetweenBurstShots, int burst, int baseDamage, double armorPenetration,
            double maxRange,
            double accuracyTouch, double accuracyShort, double accuracyMedium, double accuracyLong,
            double forcedMissRadius, double explosionRadius, LeavesBehind leavesBehind = LeavesBehind.Nothing)
        {
            Name = name;
            AmmoType = ammoType;
            Warmup = warmup;
            Cooldown = cooldown;
            SecondsBetweenBurstShots = secondsBetweenBurstShots;
            Burst = burst;
            BaseDamage = baseDamage;
            ArmorPenetration = armorPenetration;
            MaxRange = maxRange;
            AccuracyTouch = accuracyTouch;
            AccuracyShort = accuracyShort;
            AccuracyMedium = accuracyMedium;
            AccuracyLong = accuracyLong;
            ForcedMissRadius = forcedMissRadius;
            ExplosionRadius = explosionRadius;
            LeavesBehind = leavesBehind;
        }


        public string Name { get; }
        public AmmoType AmmoType { get; }
        public double Warmup { get; }
        public double Cooldown { get; }
        public double SecondsBetweenBurstShots { get; }
        
        public int Burst { get; }

        public int BaseDamage { get; }

        public double ArmorPenetration { get; }

        public double MaxRange { get; }

        public double AccuracyTouch { get; }
        public double AccuracyShort { get; }
        public double AccuracyMedium { get; }
        public double AccuracyLong { get; }
        public double ForcedMissRadius { get; }
        public double ExplosionRadius { get; }
        public LeavesBehind LeavesBehind { get; }

        public double AccuracyAt(int range)
        {
            if (range > MaxRange)
                return 0;

            if (range < 3)
            {
                return 0;
            }
            else if (range < 12)
            {
                float ratio = (range - 3f) / (12f - 3f);
                return (1 - ratio) * AccuracyTouch + ratio * AccuracyShort;
            }
            else if (range < 25)
            {
                float ratio = (range - 12f) / (25f - 12f);
                return (1 - ratio) * AccuracyShort + ratio * AccuracyMedium;
            }
            else if (range < 40)
            {
                float ratio = (range - 25f) / (40f - 25f);
                return (1 - ratio) * AccuracyMedium + ratio * AccuracyLong;
            }
            else
            {
                return AccuracyLong;
            }
        }

        public double ForcedAccuracyAt(int range)
        {
            if (range > MaxRange)
                return 0.0;
         
            // 0.5 => 0.9
            // 3.0 => 0.3
            var forcedAccuracyAt = (ForcedMissRadius - 0.5) / (3.0 - 0.5) * (0.9 - 0.3) + 0.3;
            return Math.Max(forcedAccuracyAt, 0.1);
        }
    }
}