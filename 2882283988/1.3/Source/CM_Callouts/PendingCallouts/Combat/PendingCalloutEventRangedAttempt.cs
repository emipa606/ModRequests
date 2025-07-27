using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Combat
{
	// Token: 0x02000020 RID: 32
	public class PendingCalloutEventRangedAttempt : PendingCalloutEventDoublePawn
	{
		// Token: 0x06000049 RID: 73 RVA: 0x00003D43 File Offset: 0x00001F43
		public PendingCalloutEventRangedAttempt(Pawn _initiator, Pawn _recipient, Verb_LaunchProjectile _verb) : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack, null)
		{
			this.verb = _verb;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003D64 File Offset: 0x00001F64
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			bool flag = this.recipient != null;
			if (flag)
			{
				CalloutUtility.CollectCoverRules(this.recipient, this.initiator, "INITIATOR_COVER", this.verb, ref result);
				CalloutUtility.CollectCoverRules(this.initiator, this.recipient, "RECIPIENT_COVER", this.verb, ref result);
			}
			return result;
		}

		// Token: 0x04000056 RID: 86
		public Verb_LaunchProjectile verb = null;
	}
}
