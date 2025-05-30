namespace Euphoric.YayosAmmunitionPatch
{
    public class GunAmmoSetting
    {
        public GunAmmoSetting(double averageAccuracy, double armorPenetrationRating, double explosionRating, double effectiveDamage, int ammoPerShot,
            double shotsPerMinute, double ammoPerMinute, int gunShots, string warning)
        {
            AverageAccuracy = averageAccuracy;
            ArmorPenetrationRating = armorPenetrationRating;
            ExplosionRating = explosionRating;
            EffectiveDamage = effectiveDamage;
            AmmoPerShot = ammoPerShot;
            ShotsPerMinute = shotsPerMinute;
            AmmoPerMinute = ammoPerMinute;
            GunShots = gunShots;
            Warning = warning;
        }
        public double AverageAccuracy { get; }
        public double ArmorPenetrationRating { get; }
        public double ExplosionRating { get; }

        public double EffectiveDamage { get; }
        public int AmmoPerShot { get; }
        public double ShotsPerMinute { get; }
        public double AmmoPerMinute { get; }
        public int GunShots { get; }
        
        public string Warning { get; set; }
    }
}