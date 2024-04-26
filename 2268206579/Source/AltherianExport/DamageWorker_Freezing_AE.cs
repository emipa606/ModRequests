using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
	// Adds freezing related effects to Freezing_AE damage
	public class DamageWorker_Freezing_AE : DamageWorker_AddInjury
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			bool flag = (pawn != null);
			if (flag && pawn.Faction == Faction.OfPlayer) Find.TickManager.slower.SignalForceNormalSpeedShort();
			if (flag && pawn.RaceProps.IsMechanoid) dinfo.SetAmount(0);
			Map map = victim.Map;
			DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);

			if (victim.Destroyed && map != null && pawn == null)
			{
				foreach (IntVec3 c in victim.OccupiedRect())
				{
					FilthMaker.TryMakeFilth(c, map, ThingDefOf_AE.Filth_IceCrystals_AE, 1, FilthSourceFlags.None);
				}
				Plant plant = victim as Plant;
				if (plant != null && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree)
				{
					((DeadPlant)GenSpawn.Spawn(ThingDefOf_AE.FrozenTree_AE, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
				}
			}

			return damageResult;
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
			float num = 1f;
			IntVec3 position = explosion.Position;
			bool flag = c.InBounds(explosion.Map);
			if (flag)
			{
				float lengthHorizontal = (position - c).LengthHorizontal;
				float num2 = 1f - lengthHorizontal / explosion.radius;
				explosion.Map.snowGrid.AddDepth(c, num2 * num);
			}
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			bool flag = !pawn.RaceProps.IsMechanoid;
			if (flag)
			{
				base.FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
			}
		}
	}
}
