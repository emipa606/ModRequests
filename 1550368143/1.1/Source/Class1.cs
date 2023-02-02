using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using HarmonyLib;

namespace Torgue
{
    public class Torgue_Projectile_Explosive : Projectile_Explosive
    {
        private int ticksToDetonation = 0;

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
            Find.BattleLog.Add(battleLogEntry_RangedImpact);
            if (hitThing != null)
            {
                DamageDef damageDef = this.def.projectile.damageDef;
                float amount = (float)base.DamageAmount;
                float armorPenetration = base.ArmorPenetration;
                float y = this.ExactRotation.eulerAngles.y;
                Thing launcher = this.launcher;
                ThingDef equipmentDef = this.equipmentDef;
                DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetration, y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
                {
                    pawn.stances.StaggerFor(95);
                }
            }
            else
            {
                SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
                MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                if (base.Position.GetTerrain(map).takeSplashes)
                {
                    MoteMaker.MakeWaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
                }
            }

            if (this.def.projectile.explosionDelay == 0)
            {
                GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
                this.Explode();
                return;
            }
            this.landed = true;
            this.ticksToDetonation = this.def.projectile.explosionDelay;
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, this.def.projectile.damageDef, this.launcher.Faction);

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToDetonation, "ticksToDetonation", 0, false);
        }
    }
    public class Torgue_DamageWorker_AddInjury : DamageWorker_AddInjury
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }
            MoteMaker.MakeStaticMote(explosion.Position, explosion.Map, ThingDefOf.Mote_ExplosionFlash, explosion.radius * 6f);
            this.ExplosionVisualEffectCenter(explosion);
        }
    }
    public class Verb_Properties_ShotGun : VerbProperties
    {
        public int pelletCount=1;
    }
    public class Verb_Shoot_ShotGun : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            bool flag = base.TryCastShot();
            Verb_Properties_ShotGun props = (Verb_Properties_ShotGun)verbProps;
            if (flag && (props.pelletCount - 1 > 0))
            {
                for (int index = 1; index < props.pelletCount; index++)
                {
                    base.TryCastShot();
                }
            }
            return flag;
        }
    }
    public class CompClusterBombProperties : CompProperties
    {
        public ThingDef subProjectile;
        public int clusterCount;
        public int clusterRadius = 3;
    }

    public class TorgueProjectileProps:ProjectileProperties
    {
        public ThingDef subProjectile;
        public int clusterCount;
        public int clusterRadius = 1;
    }
    public class Projectile_Torgue_Rocket : Projectile
    {
        public TorgueProjectileProps Props
        {
            get
            {
                return (TorgueProjectileProps)def.projectile;
            }
        }

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            IntVec3 position = base.Position;
            Map map2 = map;
            float explosionRadius = this.def.projectile.explosionRadius;
            DamageDef explosionDamageType = def.projectile.damageDef;
            Thing launcher = this.launcher;
            int damageAmount = base.DamageAmount;
            float armorPenetration = base.ArmorPenetration;
            ThingDef equipmentDef = this.equipmentDef;
            GenExplosion.DoExplosion(position, map2, explosionRadius, explosionDamageType, launcher, damageAmount, armorPenetration, null, equipmentDef, this.def, this.intendedTarget.Thing, null, 0f, 1, false, null, 0f, 1, 0f, false);
            for (int index = 0; index < Props.clusterCount; index++)
            {
                int max = GenRadial.NumCellsInRadius(Props.clusterRadius);
                int num;
                if (Props.clusterRadius > 4)
                {
                    num = Rand.Range(2, max);
                }
                else
                {
                    num = Rand.Range(0, max);
                }

                IntVec3 c = Position + GenRadial.RadialPattern[num];
                Projectile_Explosive projectile = (Projectile_Explosive)ThingMaker.MakeThing(Props.subProjectile, null);
                GenSpawn.Spawn(projectile, position, map);
                projectile.Launch(this, position.ToVector3(), c, c, ProjectileHitFlags.All, null, null);
            }
            base.Impact(hitThing);
        }
    }
}
