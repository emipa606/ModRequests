using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NorseWeapons
{
	public class Projectile_LightningStrike : Bullet
	{
		protected override void Impact(Thing hitThing, bool blockedbyshield = false)
		{
			Map map = base.Map;
            Pawn pawn = this.launcher as Pawn;

            CellRect cellRect = CellRect.CenteredOn(base.Position, 1);
			cellRect.ClipInsideMap(map);

			IntVec3 randomCell = cellRect.RandomCell;
			if (randomCell.IsValid && randomCell.InBounds(base.Map))
			{
				Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, randomCell));
				this.LightningBlast(0, randomCell, map, 2.2f);
			}
			Destroy();
		}

		protected void LightningBlast(int pwr, IntVec3 pos, Map map, float radius)
		{
			ThingDef def = this.def;
            SoundDef exp = null;
            Explosion(pwr, pos, map, radius, DamageDefOf.EMP, this.launcher, exp, def, this.equipmentDef, ThingDefOf.Spark, 4.4f, 1, false, null, 0f, 1);
            Explosion(pwr, pos, map, radius, DamageDefOf.Stun, this.launcher, exp, def, this.equipmentDef, ThingDefOf.Mote_Stun, 1.4f, 1, false, null, 0f, 1);
            Explosion(pwr, pos, map, radius, DamageDefOf.Bomb, this.launcher, exp, def, this.equipmentDef, ThingDefOf.Spark, 0.4f, 1, false, null, 0f, 1);
        }

		public void Explosion(int pwr, IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, SoundDef explosionSound = null, ThingDef projectile = null, ThingDef source = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1)
		{
			System.Random rnd = new System.Random();
            if (pwr >= 1)
            {
                radius = (float)(rnd.Next(pwr, pwr*2)/1.8);
            }
			if (map == null)
			{
				Log.Warning("Tried to do explosion in a null map.");
				return;
			}
            Explosion explosion = (Explosion)GenSpawn.Spawn(ThingDefOf.Explosion, center, map);
            explosion.damageFalloff = true;
            explosion.chanceToStartFire = 0.0f;
            explosion.Position = center;
			explosion.radius = radius;
			explosion.damType = damType;
			explosion.instigator = instigator;
			explosion.damAmount = GenMath.RoundRandom((float)damType.defaultDamage);
			explosion.weapon = source;
			explosion.preExplosionSpawnThingDef = preExplosionSpawnThingDef;
			explosion.preExplosionSpawnChance = preExplosionSpawnChance;
			explosion.preExplosionSpawnThingCount = preExplosionSpawnThingCount;
			explosion.postExplosionSpawnThingDef = postExplosionSpawnThingDef;
			explosion.postExplosionSpawnChance = postExplosionSpawnChance;
			explosion.postExplosionSpawnThingCount = postExplosionSpawnThingCount;
			explosion.applyDamageToExplosionCellsNeighbors = applyDamageToExplosionCellsNeighbors;
            explosion.StartExplosion(explosionSound, null);
		}
    }
}