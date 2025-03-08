using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace UniversalTaxidermy
{
	public static class Utility
	{
		public static void MakeTaleFor(Pawn pawn)
		{
			List<TaleDef> new_tales = DefDatabase<TaleDef>.AllDefs.Where((TaleDef td) => td.usableForArt && td.taleClass == typeof(Tale_SinglePawn)).ToList();
			TaleDef new_tale = new_tales[Rand.RangeInclusive(0, new_tales.Count() - 1)];
			Tale tale = TaleFactory.MakeRawTale(new_tale, new object[] { pawn });
			Find.TaleManager.Add(tale);
		}
	}
}
