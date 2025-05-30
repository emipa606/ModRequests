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
	public class DamageWorker_KnockBack : DamageWorker_AddInjury
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			if (pawn != null)
            {
				var instigator = dinfo.Instigator;
				var cells = GenRadial.RadialCellsAround(pawn.Position, 3, true).Where(x => x.DistanceTo(victim.Position) >= 3 && x.DistanceTo(victim.Position) < x.DistanceTo(instigator.Position));
				var cell = cells.RandomElement();
				victim.Position = cell;
				pawn.stances.stunner.StunFor(180, dinfo.Instigator);
				return new DamageResult();
			}
			else
            {
				return new DamageResult
				{
					totalDamageDealt = 20f
				};
            }
		}
	}
}