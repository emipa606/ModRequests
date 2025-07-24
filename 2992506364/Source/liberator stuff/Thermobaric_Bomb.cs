using System;
using Verse;
using RimWorld;

namespace liberator_stuff
{
	// Token: 0x02001370 RID: 4976
	public class Projectile_shelRAin : Projectile
	{
		// Token: 0x1700153B RID: 5435
		// (get) Token: 0x06007A6A RID: 31338 RVA: 0x00002662 File Offset: 0x00000862
		public override bool AnimalsFleeImpact
		{
			get
			{
				return true;
			}
		}

		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			Map map = base.Map;
			base.Impact(hitThing, blockedByShield);
			IntVec3 position = base.Position;
			Map map2 = map;
			float explosionRadius = this.def.projectile.explosionRadius;
			
			Thing launcher = this.launcher;
			int damageAmount = this.DamageAmount;
			float armorPenetration = this.ArmorPenetration;
			SoundDef explosionSound = null;
			ThingDef equipmentDef = this.equipmentDef;
			ThingDef def = this.def;
			ThingDef Filth_Fuel = ThingDefOf.Filth_Fuel;
			GenExplosion.DoExplosion(position, map2, explosionRadius, DamageDefOf.Flame, launcher, damageAmount, armorPenetration, explosionSound, equipmentDef, def, this.intendedTarget.Thing, Filth_Fuel, 0.2f, 1, GasType.BlindSmoke, false, null, 0f, 1, 0.8f, false, null, null, null, true, 1f, 0f, true, null, 1f);
			CellRect cellRect = CellRect.CenteredOn(base.Position, 5);
			cellRect.ClipInsideMap(map);
			for (int i = 0; i < 10; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				this.DoFireExplosion(randomCell, map, 7.8f);
			}
		}

		protected void DoFireExplosion(IntVec3 pos, Map map, float radius)

		{
			DamageDef TLRS_ThermobaricBomb = TLRSDefOf.TLRS_ThermobaricBomb;
			GenExplosion.DoExplosion(pos, map, radius, TLRS_ThermobaricBomb, this.launcher, this.DamageAmount, this.ArmorPenetration, null, this.equipmentDef, this.def, this.intendedTarget.Thing, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f);
		}

		private const int ExtraExplosionCount = 10;

		private const int ExtraExplosionRadius = 9;
	}
}