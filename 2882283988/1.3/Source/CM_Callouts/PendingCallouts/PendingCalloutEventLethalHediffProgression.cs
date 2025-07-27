using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
	// Token: 0x0200001B RID: 27
	public class PendingCalloutEventLethalHediffProgression : PendingCalloutEventSinglePawn
	{
		// Token: 0x0600003D RID: 61 RVA: 0x00003A42 File Offset: 0x00001C42
		public PendingCalloutEventLethalHediffProgression(Pawn _initiator, Hediff _hediff) : base(CalloutCategory.Undefined, _initiator, CalloutDefOf.CM_Callouts_RulePack_Lethal_Hediff_Progression)
		{
			this.hediff = _hediff;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003A64 File Offset: 0x00001C64
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			CalloutUtility.CollectHediffRules(this.hediff, "CULPRITHEDIFF", ref result);
			return result;
		}

		// Token: 0x04000052 RID: 82
		public Hediff hediff = null;
	}
}
