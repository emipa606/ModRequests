using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LobotomyCorp.Weapon
{
    public class LCBullet : Bullet
    {
		private HashSet<Thing> cured = new HashSet<Thing>();

        public override void Tick()
        {
			CompProperties_BulletExtend comp = def.GetCompProperties<CompProperties_BulletExtend>();
			if (comp.healingColonist)
			{
				List<Thing> things = Map.thingGrid.ThingsListAt(Position).FindAll(
					(Thing thing) => thing is Pawn 
						&& thing.Faction.IsPlayer && !cured.Contains(thing)
					);

				foreach (Thing hitThing in things)
				{
					Pawn p = hitThing as Pawn;
					if (p.health.hediffSet.GetInjuriesTendable() != null && p.health.hediffSet.GetInjuriesTendable().Count() > 0) //GetHediffs<Hediff_Injury>().EnumerableNullOrEmpty())
						p.health.hediffSet.GetInjuriesTendable().RandomElement().Heal(comp.healValue);
					cured.Add(hitThing);
				}
			}

            base.Tick();
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			cured.Clear();

			CompProperties_BulletExtend comp = def.GetCompProperties<CompProperties_BulletExtend>();
			if (comp == null)
			{
				base.Impact(hitThing);
				return;
			}

			if (!blockedByShield && this.def.projectile.landedEffecter != null)
			{
				this.def.projectile.landedEffecter.Spawn(base.Position, base.Map, 1f).Cleanup();
			}
			if (!blockedByShield && !this.def.projectile.soundImpact.NullOrUndefined())
			{
				this.def.projectile.soundImpact.PlayOneShot(SoundInfo.InMap(this, MaintenanceType.None));
			}

			Map map = Map;
			IntVec3 position = base.Position;

			// ここで殺してるので、これより上でMapとか取らないともれなくしぬ
			GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
			Destroy();

			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = 
				new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			NotifyImpact(hitThing, map, position);

			if (hitThing != null)
			{
				List<IntVec3> cells = comp.AffectCellsOf(position);
				DamageApplyer.CacheThingLooper.Loop(cells,map,(Thing t)=>
				{
					
					if (comp.healingColonist && hitThing is Pawn && hitThing.Faction.IsPlayer)
					{
						/*
						Pawn p = hitThing as Pawn;
						if (!p.health.hediffSet.GetHediffs<Hediff_Injury>().EnumerableNullOrEmpty())
							p.health.hediffSet.GetHediffs<Hediff_Injury>().RandomElement().Heal(comp.healValue);
						*/
					}
					else
					{
						if (t is Pawn &&
						!Setting.LCModSetting.canDamageColonist && ((Pawn)t).IsColonist) return;
						else if (t == launcher) return;
						TakeDamage(t, battleLogEntry_RangedImpact);
					}
				});

			}

			SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
			if (base.Position.GetTerrain(map).takeSplashes)
			{
				FleckMaker.WaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)base.DamageAmount) * 1f, 4f);
				return;
			}
			FleckMaker.Static(this.ExactPosition, map, FleckDefOf.ShotHit_Dirt, 1f);

		}

		private void TakeDamage(Thing t, LogEntry_DamageResult res)
		{
			DamageInfo dinfo = new DamageInfo(def.projectile.damageDef,
				(float)base.DamageAmount, base.ArmorPenetration,
				ExactRotation.eulerAngles.y, launcher,
				null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown,
				intendedTarget.Thing);
			t.TakeDamage(dinfo).AssociateWithLog(res);

			Pawn pawn = t as Pawn;
			if (pawn != null && pawn.stances != null && pawn.BodySize <= this.def.projectile.StoppingPower + 0.001f)
			{
				pawn.stances.stagger.StaggerFor(95);
			}
			if (this.def.projectile.extraDamages == null)
			{
				return;
			}

			using (List<ExtraDamage>.Enumerator enumerator = this.def.projectile.extraDamages.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ExtraDamage current = enumerator.Current;
					if (Rand.Chance(current.chance))
					{
						DamageInfo dinfo2 = new DamageInfo(current.def, current.amount, current.AdjustedArmorPenetration(), this.ExactRotation.eulerAngles.y, this.launcher, null, this.equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, this.intendedTarget.Thing, true, true);
						t.TakeDamage(dinfo2).AssociateWithLog(res);
					}
				}
				return;
			}
		}

        
		private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
		{
			BulletImpactData impactData = new BulletImpactData
			{
				bullet = this,
				hitThing = hitThing,
				impactPosition = position
			};
			if (hitThing != null) hitThing.Notify_BulletImpactNearby(impactData);
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c = position + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					foreach (Thing t in c.GetThingList(map))
                    {
						if( t != hitThing ) t.Notify_BulletImpactNearby(impactData);
					}
				}
			}
		}


	}


}
