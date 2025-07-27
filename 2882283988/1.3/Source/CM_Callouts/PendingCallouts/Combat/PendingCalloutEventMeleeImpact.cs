using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Combat
{
	// Token: 0x0200001F RID: 31
	public class PendingCalloutEventMeleeImpact : PendingCalloutEventDoublePawn
	{
		// Token: 0x06000047 RID: 71 RVA: 0x00003CDF File Offset: 0x00001EDF
		public PendingCalloutEventMeleeImpact(Pawn _initiator, Pawn _recipient) : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Landed, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack_Received)
		{
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003CF8 File Offset: 0x00001EF8
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			result.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("PART", this.body, this.bodyPartsDamaged, this.bodyPartsDestroyed, result.Constants));
			return result;
		}
	}
}
