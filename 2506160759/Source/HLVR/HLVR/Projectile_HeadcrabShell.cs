using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace HLVRMonsters
{
    public class Projectile_HeadcrabShell : Projectile_Explosive
    {
        public ThingDef_HeadcrabShell Def
        {
            get
            {
                return this.def as ThingDef_HeadcrabShell;
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = true)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2, null);
            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(HLVRDefOf.HLVR_HeadcrabShellImpacted, null), loc, base.Map, WipeMode.FullRefund);
            base.Impact(hitThing);
            Explode();
            base.Destroy();
        }
        protected override void Explode()
        {
            Map map = base.Map;
            if (base.def.projectile.explosionEffect != null)
            {
                Effecter effecter = base.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(base.Position, map, base.def.projectile.explosionRadius, base.def.projectile.damageDef, base.launcher, base.DamageAmount, base.ArmorPenetration, HLVRDefOf.HeadcrabShell_Explode, base.equipmentDef, base.def, intendedTarget.Thing, base.def.projectile.postExplosionSpawnThingDef, base.def.projectile.postExplosionSpawnChance, base.def.projectile.postExplosionSpawnThingCount, preExplosionSpawnThingDef: base.def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: base.def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: base.def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: base.def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: base.def.projectile.explosionChanceToStartFire, damageFalloff: base.def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination));
        }
    }

    public class ThingDef_HeadcrabShell : ThingDef
    {
        public string HeadcrabType;
        public int HeadcrabCount;
    }
}
