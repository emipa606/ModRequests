using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class ExplosionData
    {
        public float explosiveRadius = 1.9f;

        public DamageDef explosiveDamageType;

        public int damageAmountBase = -1;

        public float armorPenetrationBase = -1f;

        public ThingDef postExplosionSpawnThingDef;

        public float postExplosionSpawnChance;

        public int postExplosionSpawnThingCount = 1;

        public bool applyDamageToExplosionCellsNeighbors = true;

        public ThingDef preExplosionSpawnThingDef;

        public float preExplosionSpawnChance;

        public int preExplosionSpawnThingCount = 1;

        public float chanceToStartFire;

        public bool damageFalloff;

        public GasType? postExplosionGasType;

        public bool doVisualEffects = true;

        public bool doSoundEffects = true;

        public float propagationSpeed = 1f;

        public EffecterDef explosionEffect;

        public SoundDef explosionSound;

        public void Detonate(Map map, IntVec3 cell, Thing instigator, List<Thing> ignoredThings = null)
        {
            if (explosionEffect != null)
            {
                Effecter effecter = explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(cell, map), new TargetInfo(cell, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(cell, map, explosiveRadius, explosiveDamageType, instigator, damageAmountBase, armorPenetrationBase, explosionSound, null, null, null, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, chanceToStartFire, damageFalloff, null, ignoredThings, null, doVisualEffects, doSoundEffects: doSoundEffects, propagationSpeed: propagationSpeed);
        }
    }
}
