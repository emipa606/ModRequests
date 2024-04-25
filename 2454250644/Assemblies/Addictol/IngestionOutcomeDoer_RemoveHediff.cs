using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	// Token: 0x020008BE RID: 2238
	public class IngestionOutcomeDoer_RemoveHediff : IngestionOutcomeDoer
	{
		// Token: 0x060036BC RID: 14012 RVA: 0x0012A458 File Offset: 0x00128658
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
			float num;
			if (this.severity > 0f)
			{
				num = this.severity;
			}
			else
			{
				num = this.hediffDef.initialSeverity;
			}
			if (this.divideByBodySize)
			{
				num /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
			hediff.Severity = num;
			pawn.health.AddHediff(hediff, null, null, null);
		}

		// Token: 0x060036BD RID: 14013 RVA: 0x0012A4D2 File Offset: 0x001286D2
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && this.chance >= 1f)
			{
				foreach (StatDrawEntry statDrawEntry in this.hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
				{
					yield return statDrawEntry;
				}
				IEnumerator<StatDrawEntry> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x04001DD0 RID: 7632
		public HediffDef hediffDef;

		// Token: 0x04001DD1 RID: 7633
		public float severity = -1f;

		// Token: 0x04001DD2 RID: 7634
		public ChemicalDef toleranceChemical;

		// Token: 0x04001DD3 RID: 7635
		private bool divideByBodySize;
	}
}
