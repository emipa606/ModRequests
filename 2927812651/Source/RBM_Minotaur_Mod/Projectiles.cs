using Verse;

namespace RBM_Minotaur
{
    // Bullet type that applies the terrified state in an area - used by Taurail Gun
    public class Projectile_ObsidianBullet : Projectile_Explosive
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Projectile) protected override void Impact(Thing hitThing, bool blockedByShield = false)"); }
            Map map = base.Map;
            base.Impact(hitThing);
            IntVec3 position = base.Position;
            float explosionRadius = MinotaurSettings.TaurailFearRadius;
            DamageDef damageDef = this.def.projectile.damageDef;
            int damageAmount = this.DamageAmount;
            float armorPenetration = this.ArmorPenetration;

            GenExplosion.DoExplosion(position, map, explosionRadius, damageDef, null, damageAmount, armorPenetration, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
            RBM_Utils.TerrifyInArea(position, map, explosionRadius);
        }
    }
}
