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
	public static class DamageDefOf_AE
	{
		static DamageDefOf_AE()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf_AE));
		}

		public static DamageDef Freezing_AE;
		public static DamageDef Electroshock_AE;
		public static DamageDef Plasma_AE;
		public static DamageDef Toxic_AE;
		public static DamageDef Corrosive_AE;
		public static DamageDef Sonic_AE;
	}
}
