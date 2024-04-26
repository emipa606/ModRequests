using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
	public class DamageWorker_Toxic_AE : DamageWorker_AddInjury
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}

			DamageInfo dinfoRef = dinfo;
			if (pawn != null && pawn.RaceProps.IsMechanoid)
            {
				dinfoRef.SetAmount(0);
			}

			DamageWorker.DamageResult damageResult = base.Apply(dinfoRef, victim);

			return damageResult;
		}
	}
}
