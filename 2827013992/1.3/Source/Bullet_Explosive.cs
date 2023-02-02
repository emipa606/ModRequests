using RimWorld;
using Verse;

namespace Primaris
{
    public class Bullet_Explosive : Bullet
    {
        public override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(base.Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, 
                (int)(base.DamageAmount / 2f), base.ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, def.projectile.postExplosionSpawnChance, def.projectile.postExplosionSpawnThingCount, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination));
            base.Impact(hitThing);
        }
    }
}
