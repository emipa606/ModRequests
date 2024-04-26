using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AltherianExport
{
    [DefOf]
	public static class HediffDefOf_AE
    {
		static HediffDefOf_AE()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_AE));
		}

		// Weapon/damage hediffs
		public static HediffDef ToxicIllness_AE;

		// for reference
		public static HediffDef HearingLoss;

	}
}
