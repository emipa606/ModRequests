using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
	public class DamageWorker_Corrosive_AE : DamageWorker_AddInjury
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}

			DamageInfo dinfo2 = dinfo;
			if (pawn != null)
            {
				if (pawn.RaceProps.IsMechanoid)
                {
					dinfo2.SetAmount(dinfo.Amount * 2f);
				}
            }
			
			DamageWorker.DamageResult damageResult = base.Apply(dinfo2, victim);

			return damageResult;
		}
	}
}
