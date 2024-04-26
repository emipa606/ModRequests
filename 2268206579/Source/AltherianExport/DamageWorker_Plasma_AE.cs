using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
	// Initally added to ensure radiation illness hediff would not affect mechanoids- pretty much obsolete
	public class DamageWorker_Plasma_AE : DamageWorker_AddInjury
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			bool flag = pawn != null && pawn.Faction == Faction.OfPlayer;
			if (flag)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			Map map = victim.Map;
			DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);
			bool flag2 = !damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(victim));
			if (flag2)
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			bool flag3 = victim.Destroyed && map != null && pawn == null;
			if (flag3)
			{
				foreach (IntVec3 c in victim.OccupiedRect())
				{
					FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash, 1, FilthSourceFlags.None);
				}
				Plant plant = victim as Plant;
				bool flag4 = plant != null && victim.def.plant.IsTree && plant.LifeStage != PlantLifeStage.Sowing && victim.def != ThingDefOf.BurnedTree;
				if (flag4)
				{
					((DeadPlant)GenSpawn.Spawn(ThingDefOf.BurnedTree, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
				}
			}
			return damageResult;
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
			bool flag = this.def == DamageDefOf_AE.Plasma_AE && Rand.Chance(FireUtility.ChanceToStartFireIn(c, explosion.Map));
			if (flag)
			{
				FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
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
