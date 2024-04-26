using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
	// Adds stunning effects (along with StunHandler patch) to Electroshock_AE
	public class DamageWorker_Electroshock_AE : DamageWorker
	{
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);
			damageResult.stunned = true;
			return damageResult;
		}

		// StunHandler deals with electroshock's unique stun via harmony patch- old method obsolete
	}
}
