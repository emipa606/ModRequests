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
	public class DamageWorker_HammerHead : DamageWorker_AddInjury
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			if (dinfo.Instigator is Pawn attacker)
            {
				var comp = attacker.TryGetComp<CompAttackCooldown>();
				if (!comp.HasCooldownFor(AttackType.HammerHead) && Rand.Chance(0.02f))
                {
					victim.Destroy();
					GenExplosion.DoExplosion(attacker.Position, attacker.Map, 10, DamageDefOf.Bomb, attacker, 25, ignoredThings: new List<Thing> { attacker });
					comp.SetCooldown(AttackType.HammerHead, Find.TickManager.TicksGame + 7200);
				}
            }
			return new DamageResult();
		}
	}
}