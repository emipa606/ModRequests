using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FF_Drugs
{
	[DefOf]
	public static class RemediesDefOf
	{
		public static HediffDef FF_EctostasisHigh;
		static RemediesDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}
	}
}
