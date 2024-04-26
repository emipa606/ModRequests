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
	public static class Filth_AE
	{
		static Filth_AE()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(Filth_AE));
		}

		public static ThingDef Filth_IceCrystals_AE;
	}
}
