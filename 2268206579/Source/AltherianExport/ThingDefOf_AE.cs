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
	public static class ThingDefOf_AE
	{
		static ThingDefOf_AE()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_AE));
		}

		// Filth
		public static ThingDef Filth_IceCrystals_AE;

		// Plants
		public static ThingDef FrozenTree_AE;

		// Motes
		public static ThingDef Mote_ImpactSound;

	}
}
