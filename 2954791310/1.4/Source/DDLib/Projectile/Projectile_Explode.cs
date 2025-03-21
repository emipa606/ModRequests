﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD
{
	//Create explosion but no fire.
	public class DD_Projectile_Explode : Projectile
	{
		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			Ignite();
		}

		protected virtual void Ignite()
		{
			Map map = Map;
			Destroy();
			var radius = def.projectile.explosionRadius;
			var cellsToAffect = SimplePool<List<IntVec3>>.Get();
			cellsToAffect.Clear();
			cellsToAffect.AddRange(def.projectile.damageDef.Worker.ExplosionCellsToHit(Position, map, radius));

			FleckMaker.Static(Position, map, FleckDefOf.ExplosionFlash, radius * 4f);
			for (int i = 0; i < 4; i++)
			{
				FleckMaker.ThrowSmoke(Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), map, radius * 0.6f);
			}


			//Fire explosion should be tiny.
			if (this.def.projectile.explosionEffect != null)
			{
				Effecter effecter = this.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(this.Position, map, false), new TargetInfo(this.Position, map, false));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(this.Position, map, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, this.def.projectile.GetDamageAmount(1, null), this.def.projectile.GetArmorPenetration(1, null), this.def.projectile.soundExplode, this.equipmentDef, this.def, null, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.postExplosionSpawnChance, this.def.projectile.postExplosionSpawnThingCount, null, this.def.projectile.applyDamageToExplosionCellsNeighbors, this.def.projectile.preExplosionSpawnThingDef, this.def.projectile.preExplosionSpawnChance, this.def.projectile.preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff);
		}

		public override Quaternion ExactRotation
		{
			get
			{
				var forward = destination - origin;
				forward.y = 0;
				return Quaternion.LookRotation(forward);
			}
		}
	}
}
