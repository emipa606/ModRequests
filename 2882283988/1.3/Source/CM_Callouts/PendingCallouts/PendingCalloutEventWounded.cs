using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
	// Token: 0x0200001D RID: 29
	public class PendingCalloutEventWounded : PendingCalloutEventSinglePawn
	{
		// Token: 0x06000043 RID: 67 RVA: 0x00003BFA File Offset: 0x00001DFA
		public PendingCalloutEventWounded(Pawn _initiator) : base(CalloutCategory.Combat, _initiator, CalloutDefOf.CM_Callouts_RulePack_Wounded)
		{
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00003C0C File Offset: 0x00001E0C
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			result.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("PART", this.body, this.bodyPartsDamaged, this.bodyPartsDestroyed, result.Constants));
			return result;
		}
	}
}
