using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class IngestionOutcomeDoer_Impregnate : IngestionOutcomeDoer
	{
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
			if (compEggLayer != null) {
				compEggLayer.Fertilize (null);
			} else if(pawn.gender==Gender.Female) {
				Hediff preggers = HediffMaker.MakeHediff (HediffDefOf.Pregnant, pawn, null);
				typeof(Hediff).GetField ("visible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue (preggers, true);
				pawn.health.AddHediff (preggers);
			}
		}
	}
}