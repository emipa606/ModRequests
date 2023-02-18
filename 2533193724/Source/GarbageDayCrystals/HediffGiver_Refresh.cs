using System;
using Verse;

namespace Crystosentient
{
	// Token: 0x02000005 RID: 5
	public class HediffGiver_Refresh : HediffGiver
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00002DAC File Offset: 0x00000FAC
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(this.hediff, false);
			bool flag = firstHediffOfDef != null;
			if (flag)
			{
				firstHediffOfDef.ageTicks = 0;
			}
			else
			{
				base.TryApply(pawn, null);
			}
		}
	}
}
