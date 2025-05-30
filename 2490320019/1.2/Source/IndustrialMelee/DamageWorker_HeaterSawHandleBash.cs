using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IndustrialMelee
{
	public class DamageWorker_HeaterSawHandleBash : DamageWorker_AddInjury
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			Map map = victim.Map;
			DamageResult damageResult = base.Apply(dinfo, victim);
			if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(0.1f))
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			return damageResult;
		}

		public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
		{
			base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
			if (def == DamageDefOf.Flame && Rand.Chance(0.1f))
			{
				FireUtility.TryStartFireIn(c, explosion.Map, Rand.Range(0.2f, 0.6f));
			}
		}
	}
}